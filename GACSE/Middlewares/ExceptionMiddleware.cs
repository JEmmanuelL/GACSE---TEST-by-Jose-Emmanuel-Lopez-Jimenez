using GACSE.Application.DTOs;
using System.Net;
using System.Text.Json;

namespace GACSE.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, mensaje) = exception switch
            {
                KeyNotFoundException ex => (HttpStatusCode.NotFound, ex.Message),
                ArgumentException ex => (HttpStatusCode.BadRequest, ex.Message),
                InvalidOperationException ex when ex.Message.StartsWith("{") =>
                    (HttpStatusCode.Conflict, ex.Message),
                InvalidOperationException ex => (HttpStatusCode.Conflict, ex.Message),
                _ => (HttpStatusCode.InternalServerError, "Ocurrió un error interno en el servidor.")
            };

            context.Response.StatusCode = (int)statusCode;

            // Si es un conflicto de horario con sugerencias (JSON serializado)
            if (statusCode == HttpStatusCode.Conflict && mensaje.StartsWith("{"))
            {
                await context.Response.WriteAsync(mensaje);
                return;
            }

            var respuesta = new
            {
                error = true,
                statusCode = (int)statusCode,
                mensaje
            };

            var json = JsonSerializer.Serialize(respuesta, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
