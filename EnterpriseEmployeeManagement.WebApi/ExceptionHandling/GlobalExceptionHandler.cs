using EnterpriseEmployeeManagement.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseEmployeeManagement.WebApi.ExceptionHandling
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Log the complete exception including InnerException
            _logger.LogError(
                exception,
                "Unhandled exception occurred. Method: {Method}, Path: {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            var statusCode = exception switch
            {
                NotFoundException =>
                    StatusCodes.Status404NotFound,

                ConflictException =>
                    StatusCodes.Status409Conflict,

                DomainException =>
                    StatusCodes.Status400BadRequest,

                UnauthorizedAccessException =>
                    StatusCodes.Status401Unauthorized,

                _ =>
                    StatusCodes.Status500InternalServerError
            };

            var title = statusCode switch
            {
                StatusCodes.Status400BadRequest =>
                    "Bad Request",

                StatusCodes.Status401Unauthorized =>
                    "Unauthorized",

                StatusCodes.Status404NotFound =>
                    "Resource Not Found",

                StatusCodes.Status409Conflict =>
                    "Conflict",

                _ =>
                    "Internal Server Error"
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Instance = httpContext.Request.Path
            };

            // =====================================================
            // DEVELOPMENT
            // =====================================================

            if (_environment.IsDevelopment())
            {
                // Show the SQL Server/EF Core inner exception
                // instead of only "An error occurred while saving..."
                problemDetails.Detail =
                    exception.InnerException?.Message
                    ?? exception.Message;

                problemDetails.Extensions["exceptionType"] =
                    exception.GetType().Name;

                problemDetails.Extensions["innerExceptionType"] =
                    exception.InnerException?.GetType().Name;

                problemDetails.Extensions["stackTrace"] =
                    exception.StackTrace;

                if (exception.InnerException != null)
                {
                    problemDetails.Extensions["innerExceptionMessage"] =
                        exception.InnerException.Message;
                }
            }
            else
            {
                // Never expose internal exception details in production
                problemDetails.Detail = statusCode == 500
                    ? "An unexpected error occurred."
                    : exception.Message;
            }

            httpContext.Response.StatusCode = statusCode;

            httpContext.Response.ContentType =
                "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken);

            return true;
        }
    }
}