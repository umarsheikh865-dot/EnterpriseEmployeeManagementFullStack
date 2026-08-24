using System.Diagnostics;

namespace EnterpriseEmployeeManagement.WebApi.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            var requestId = context.TraceIdentifier;

            using (_logger.BeginScope(
                "RequestId: {RequestId}",
                requestId))
            {
                _logger.LogInformation(
                    "Request started. Method: {Method}, Path: {Path}",
                    context.Request.Method,
                    context.Request.Path);

                try
                {
                    await _next(context);
                }
                finally
                {
                    stopwatch.Stop();

                    _logger.LogInformation(
                        "Request completed. Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, DurationMs: {DurationMs}",
                        context.Request.Method,
                        context.Request.Path,
                        context.Response.StatusCode,
                        stopwatch.ElapsedMilliseconds);
                }
            }
        }
    }
}