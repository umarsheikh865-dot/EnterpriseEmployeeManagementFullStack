namespace EnterpriseEmployeeManagement.WebApi.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers["X-Content-Type-Options"] =
                "nosniff";

            context.Response.Headers["X-Frame-Options"] =
                "DENY";

            context.Response.Headers["Referrer-Policy"] =
                "strict-origin-when-cross-origin";

            await _next(context);
        }
    }
}