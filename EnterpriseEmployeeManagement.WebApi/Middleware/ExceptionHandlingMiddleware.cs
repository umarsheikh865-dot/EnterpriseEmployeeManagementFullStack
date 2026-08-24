using System.Net;
using System.Text.Json;

namespace EnterpriseEmployeeManagement.WebApi.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
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
                _logger.LogError(
                    ex,
                    "Unhandled exception occurred. Request: {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            context.Response.Clear();

            context.Response.StatusCode =
                (int)HttpStatusCode.InternalServerError;

            context.Response.ContentType =
                "application/problem+json";

            var response = new
            {
                title = "Internal Server Error",
                status = 500,

                // TEMPORARY:
                // We are showing the real exception while debugging.
                detail = exception.Message,

                instance = context.Request.Path,

                exceptionType = exception.GetType().Name,

                stackTrace = exception.StackTrace
            };

            var json = JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            await context.Response.WriteAsync(json);
        }
    }
}