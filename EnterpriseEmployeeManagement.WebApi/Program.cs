using EnterpriseEmployeeManagement.Infrastructure.DependencyInjection;
using EnterpriseEmployeeManagement.Infrastructure.Persistence;
using EnterpriseEmployeeManagement.Infrastructure.Services;

using EnterpriseEmployeeManagement.WebApi.ExceptionHandling;
using EnterpriseEmployeeManagement.WebApi.HealthChecks;
using EnterpriseEmployeeManagement.WebApi.Middleware;
using EnterpriseEmployeeManagement.WebApi.Options;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;

using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
// ============================================================
// REQUIRED FOR INTEGRATION TESTING
// ============================================================


public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ============================================================
        // JWT CONFIGURATION
        // ============================================================

        var jwtSettings =
            builder.Configuration.GetSection("JwtSettings");

        var jwtKey =
            jwtSettings["Key"];

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            // Do not throw here to avoid crashing the web host during
            // development or when configuration is missing. Generate
            // a temporary in-memory key so the app can start. This is
            // NOT suitable for production — set JwtSettings:Key in
            // configuration or environment variables.
            var tempBytes = new byte[64];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(tempBytes);
            }

            jwtKey = Convert.ToBase64String(tempBytes);

            Console.WriteLine("WARNING: JwtSettings:Key is not configured. Using a temporary in-memory key. Configure JwtSettings:Key for production.");
        }

        // ============================================================
        // JWT KEY
        // ============================================================

        var keyBytes =
            Encoding.UTF8.GetBytes(jwtKey);

        if (keyBytes.Length < 32)
        {
            keyBytes =
                SHA256.HashData(keyBytes);
        }

        var securityKey =
            new SymmetricSecurityKey(keyBytes);

        // ============================================================
        // CONTROLLERS
        // ============================================================

        builder.Services.AddControllers();

        // ============================================================
        // CORS
        // ============================================================

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                "BlazorPolicy",
                policy =>
                {
                    policy
                        .WithOrigins("https://localhost:7000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });

        // ============================================================
        // HEALTH CHECKS
        // ============================================================

        builder.Services
            .AddHealthChecks()

            // --------------------------------------------------------
            // APPLICATION IS ALIVE
            // --------------------------------------------------------

            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy(),
                tags: new[] { "live" })

            // --------------------------------------------------------
            // DATABASE IS AVAILABLE (use custom check that handles InMemory provider)
            // --------------------------------------------------------
            .AddCheck<ApplicationDbContextHealthCheck>(
                "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "ready" });

        // ============================================================
        // HTTP CLIENT FACTORY + RESILIENCE
        // ============================================================

        builder.Services
            .AddHttpClient("ExternalApi")
            .AddStandardResilienceHandler();

        // ============================================================
        // EXTERNAL API SERVICE
        // ============================================================

        builder.Services.AddScoped<ExternalApiService>();

        // ============================================================
        // RATE LIMITING
        // ============================================================

        builder.Services.AddRateLimiter(options =>
        {
            // --------------------------------------------------------
            // GENERAL FIXED WINDOW POLICY
            // --------------------------------------------------------

            options.AddFixedWindowLimiter(
                "fixed",
                limiterOptions =>
                {
                    limiterOptions.PermitLimit = 20;

                    limiterOptions.Window =
                        TimeSpan.FromSeconds(10);

                    limiterOptions.QueueProcessingOrder =
                        QueueProcessingOrder.OldestFirst;

                    limiterOptions.QueueLimit = 2;
                });

            // --------------------------------------------------------
            // LOGIN RATE LIMIT POLICY
            // --------------------------------------------------------

            options.AddFixedWindowLimiter(
                "LoginPolicy",
                limiterOptions =>
                {
                    limiterOptions.PermitLimit = 5;

                    limiterOptions.Window =
                        TimeSpan.FromMinutes(1);

                    limiterOptions.QueueLimit = 0;
                });

            // --------------------------------------------------------
            // RATE LIMIT REJECTION
            // --------------------------------------------------------

            options.OnRejected =
                async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode =
                        StatusCodes.Status429TooManyRequests;

                    await context.HttpContext.Response.WriteAsync(
                        "Too many requests. Please try again later.",
                        cancellationToken);
                };
        });

        // ============================================================
        // PROBLEM DETAILS
        // ============================================================

        builder.Services.AddProblemDetails();

        // ============================================================
        // GLOBAL EXCEPTION HANDLER
        // ============================================================

        builder.Services.AddExceptionHandler<
            GlobalExceptionHandler>();

        // ============================================================
        // INFRASTRUCTURE
        // ============================================================

        builder.Services.AddInfrastructure(
            builder.Configuration);

        // ============================================================
        // OPTIONS PATTERN
        // ============================================================

        builder.Services.Configure<ApiSettings>(
            builder.Configuration.GetSection("ApiSettings"));

        // ============================================================
        // JWT AUTHENTICATION
        // ============================================================

        builder.Services
            .AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep JWT claim names exactly as they appear
                // inside the token.
                options.MapInboundClaims = false;

                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        // ------------------------------------------------
                        // ISSUER
                        // ------------------------------------------------

                        ValidateIssuer = true,

                        ValidIssuer =
                            jwtSettings["Issuer"],

                        // ------------------------------------------------
                        // AUDIENCE
                        // ------------------------------------------------

                        ValidateAudience = true,

                        ValidAudience =
                            jwtSettings["Audience"],

                        // ------------------------------------------------
                        // TOKEN EXPIRATION
                        // ------------------------------------------------

                        ValidateLifetime = true,

                        // ------------------------------------------------
                        // SIGNING KEY
                        // ------------------------------------------------

                        ValidateIssuerSigningKey = true,

                        IssuerSigningKey =
                            securityKey,

                        // ------------------------------------------------
                        // NO EXTRA TIME AFTER EXPIRATION
                        // ------------------------------------------------

                        ClockSkew =
                            TimeSpan.Zero
                    };
            });

        // ============================================================
        // AUTHORIZATION
        // ============================================================

        builder.Services.AddAuthorization(options =>
        {
            // --------------------------------------------------------
            // ADMIN ONLY
            // --------------------------------------------------------

            options.AddPolicy(
                "AdminOnly",
                policy =>
                {
                    policy.RequireRole("Admin");
                });

            // --------------------------------------------------------
            // AUTHENTICATED USER
            // --------------------------------------------------------

            options.AddPolicy(
                "AuthenticatedUser",
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                });
        });

        // ============================================================
        // SWAGGER
        // ============================================================

#if true
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            // Swagger configuration is included in source but may be disabled
            // at compile time if the OpenAPI model types are not available
            // for the current build environment. Keep code here to preserve
            // user intent.
        });
#endif

        // ============================================================
        // BUILD APPLICATION
        // ============================================================

        var app = builder.Build();

        // ============================================================
        // REQUEST LOGGING MIDDLEWARE
        // ============================================================

        app.UseMiddleware<RequestLoggingMiddleware>();

        // ============================================================
        // GLOBAL EXCEPTION HANDLER
        // ============================================================

        app.UseExceptionHandler();

        // ============================================================
        // SWAGGER
        // ============================================================

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();

            app.UseSwaggerUI();
        }

        // ============================================================
        // HTTPS
        // ============================================================

        app.UseHttpsRedirection();

        // ============================================================
        // SECURITY HEADERS
        // ============================================================

        app.UseMiddleware<SecurityHeadersMiddleware>();

        // ============================================================
        // CORS
        // ============================================================

        app.UseCors("BlazorPolicy");

        // ============================================================
        // RATE LIMITING
        // ============================================================

        app.UseRateLimiter();

        // ============================================================
        // AUTHENTICATION
        // ============================================================

        app.UseAuthentication();

        // ============================================================
        // AUTHORIZATION
        // ============================================================

        app.UseAuthorization();

        // ============================================================
        // HEALTH CHECKS
        // ============================================================

        app.MapHealthChecks(
            "/health",
            new HealthCheckOptions
            {
                ResponseWriter =
                    HealthCheckResponseWriter.WriteResponse
            });

        app.MapHealthChecks(
            "/health/live",
            new HealthCheckOptions
            {
                Predicate =
                    check =>
                        check.Tags.Contains("live"),

                ResponseWriter =
                    HealthCheckResponseWriter.WriteResponse
            });

        app.MapHealthChecks(
            "/health/ready",
            new HealthCheckOptions
            {
                Predicate =
                    check =>
                        check.Tags.Contains("ready"),

                ResponseWriter =
                    HealthCheckResponseWriter.WriteResponse
            });

        // ============================================================
        // CONTROLLERS
        // ============================================================

        app.MapControllers();

        // ============================================================
        // RUN APPLICATION
        // ============================================================

        app.Run();
    }
}

// ============================================================
// REQUIRED FOR INTEGRATION TESTING
// ============================================================

public partial class Program
{
}