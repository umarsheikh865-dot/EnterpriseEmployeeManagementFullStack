using Serilog;
using Microsoft.OpenApi;

using EnterpriseEmployeeManagement.Infrastructure.DependencyInjection;
using EnterpriseEmployeeManagement.Infrastructure.Options;
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

using FluentValidation;
using FluentValidation.AspNetCore;

using System.Text;
using System.Threading.RateLimiting;


// ============================================================
// REQUIRED FOR INTEGRATION TESTING
// ============================================================

public partial class Program
{
    private static void Main(string[] args)
    {
        // ============================================================
        // SERILOG CONFIGURATION (Enterprise Logging)
        // ============================================================
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/employee-log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Log.Information("Starting Web API Application...");

            var builder = WebApplication.CreateBuilder(args);

            // ASP.NET Core default logging ko Serilog se replace karna
            builder.Host.UseSerilog();


            // ============================================================
            // JWT CONFIGURATION
            // ============================================================

            var jwtSettings =
                builder.Configuration.GetSection("JwtSettings");

            var jwtKey =
                jwtSettings["Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException(
                    "JwtSettings:Key is not configured.");
            }

            var jwtIssuer =
                jwtSettings["Issuer"];

            if (string.IsNullOrWhiteSpace(jwtIssuer))
            {
                throw new InvalidOperationException(
                    "JwtSettings:Issuer is not configured.");
            }

            var jwtAudience =
                jwtSettings["Audience"];

            if (string.IsNullOrWhiteSpace(jwtAudience))
            {
                throw new InvalidOperationException(
                    "JwtSettings:Audience is not configured.");
            }


            // ============================================================
            // JWT KEY
            // ============================================================

            var keyBytes =
                Encoding.UTF8.GetBytes(jwtKey);

            if (keyBytes.Length < 32)
            {
                throw new InvalidOperationException(
                    "JwtSettings:Key must be at least 32 bytes long.");
            }

            var securityKey =
                new SymmetricSecurityKey(keyBytes);


            // ============================================================
            // CONTROLLERS
            // ============================================================

            builder.Services.AddControllers();


            // ============================================================
            // FLUENT VALIDATION
            // ============================================================

            builder.Services.AddValidatorsFromAssemblyContaining<Program>();
            builder.Services.AddFluentValidationAutoValidation();


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
                            .WithOrigins("http://localhost:5173", "http://localhost:5174", "https://localhost:7000")
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
                .AddCheck(
                    "self",
                    () => HealthCheckResult.Healthy(),
                    tags: new[] { "live" })
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

                options.AddFixedWindowLimiter(
                    "LoginPolicy",
                    limiterOptions =>
                    {
                        limiterOptions.PermitLimit = 5;
                        limiterOptions.Window =
                            TimeSpan.FromMinutes(1);
                        limiterOptions.QueueLimit = 0;
                    });

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
            // OPTIONS CONFIGURATION
            // ============================================================

            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection("JwtSettings"));

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
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = jwtIssuer,

                            ValidateAudience = true,
                            ValidAudience = jwtAudience,

                            ValidateLifetime = true,

                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = securityKey,

                            ClockSkew = TimeSpan.Zero
                        };
                });


            // ============================================================
            // AUTHORIZATION
            // ============================================================

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    "AdminOnly",
                    policy =>
                    {
                        policy.RequireRole("Admin");
                    });

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

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(
                    "v1",
                    new OpenApiInfo
                    {
                        Title =
                            "Enterprise Employee Management API",

                        Version =
                            "v1",

                        Description =
                            "Enterprise Employee Management Backend API"
                    });

                options.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description =
                            "Enter your JWT token. Example: Bearer {your-token}"
                    });

                options.AddSecurityRequirement(
                    document =>
                        new OpenApiSecurityRequirement
                        {
                            [
                                new OpenApiSecuritySchemeReference(
                                    "Bearer",
                                    document)
                            ] = []
                        });
            });


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
                        check => check.Tags.Contains("live"),

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
                        check => check.Tags.Contains("ready"),

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
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly!");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}


// ============================================================
// REQUIRED FOR INTEGRATION TESTING
// ============================================================

public partial class Program
{
}