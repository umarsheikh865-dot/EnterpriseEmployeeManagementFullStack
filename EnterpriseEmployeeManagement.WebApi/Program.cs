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
            // Development fallback only.
            // In Azure, configure JwtSettings:Key
            // using Application Settings / Environment Variables.

            var tempBytes = new byte[64];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tempBytes);
            }

            jwtKey = Convert.ToBase64String(tempBytes);

            Console.WriteLine(
                "WARNING: JwtSettings:Key is not configured. " +
                "Using a temporary in-memory key. " +
                "Configure JwtSettings:Key for production.");
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
                "FrontendPolicy",
                policy =>
                {
                    policy
                        // Development frontend
                        .WithOrigins(
                            "https://localhost:7000"
                        )
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
            // DATABASE
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
            // GENERAL RATE LIMIT
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
            // LOGIN RATE LIMIT
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
                // Keep JWT claim names exactly
                // as they appear in the token.

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

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen();


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

        app.UseCors("FrontendPolicy");


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
        // HEALTH CHECK
        // ============================================================

        app.MapHealthChecks(
            "/health",
            new HealthCheckOptions
            {
                ResponseWriter =
                    HealthCheckResponseWriter.WriteResponse
            });


        // ============================================================
        // LIVE HEALTH CHECK
        // ============================================================

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


        // ============================================================
        // READY HEALTH CHECK
        // ============================================================

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