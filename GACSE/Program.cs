using GACSE.Extensions;
using GACSE.Infrastructure.Data;
using GACSE.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GACSE - Mini Agenda Médica API",
        Version = "v1",
        Description = "API REST para gestionar citas médicas en un hospital. Permite administrar médicos, pacientes, citas y agendas médicas."
    });
});

// Database
builder.Services.AddDatabaseContext(builder.Configuration);

// Application Services & Repositories
builder.Services.AddApplicationServices();

var app = builder.Build();

// Aplicar migraciones automáticamente y ejecutar stored procedures
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<AppDbContext>();

    try
    {
        logger.LogInformation("Iniciando aplicación de migraciones de base de datos...");
        db.Database.Migrate();
        logger.LogInformation("Migraciones aplicadas con éxito.");

        // Validación extra: Verificar si hay datos (Seed Data)
        if (!db.Medicos.Any())
        {
            logger.LogWarning("¡ATENCIÓN! No se detectaron médicos en la base de datos tras la migración. El Seed Data podría haber fallado.");
        }
        else
        {
            var count = db.Medicos.Count();
            logger.LogInformation("Validación de datos exitosa. Se encontraron {Count} médicos en la BD.", count);
        }

        // Ejecutar stored procedures
        var spPath = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "StoredProcedures");
        if (Directory.Exists(spPath))
        {
            logger.LogInformation("Ejecutando Stored Procedures desde: {Path}", spPath);
            foreach (var sqlFile in Directory.GetFiles(spPath, "*.sql"))
            {
                var sql = File.ReadAllText(sqlFile);
                db.Database.ExecuteSqlRaw(sql);
                logger.LogInformation("Stored Procedure ejecutado: {FileName}", Path.GetFileName(sqlFile));
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ocurrió un error crítico durante la inicialización de la base de datos.");
    }
}

// Middleware de excepciones globales
app.UseMiddleware<ExceptionMiddleware>();

app.UseCors("AllowAll");

// Servir archivos estáticos del frontend (wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

// Swagger siempre habilitado (para evaluación)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GACSE API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();

app.MapControllers();

app.Run();
