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
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Ejecutar stored procedures
    try
    {
        var spPath = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "StoredProcedures");
        if (Directory.Exists(spPath))
        {
            foreach (var sqlFile in Directory.GetFiles(spPath, "*.sql"))
            {
                var sql = File.ReadAllText(sqlFile);
                db.Database.ExecuteSqlRaw(sql);
            }
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "No se pudieron ejecutar los stored procedures al iniciar. Se ejecutarán después.");
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
