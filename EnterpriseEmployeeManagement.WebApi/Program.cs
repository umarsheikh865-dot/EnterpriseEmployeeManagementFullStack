using EnterpriseEmployeeManagement.Application.Interfaces;
using EnterpriseEmployeeManagement.Application.Interfaces.Repositories;
using EnterpriseEmployeeManagement.Application.Interfaces.Services;
using EnterpriseEmployeeManagement.Application.Validators;
using EnterpriseEmployeeManagement.Infrastructure.DependencyInjection;
using EnterpriseEmployeeManagement.Infrastructure.Persistence;
using EnterpriseEmployeeManagement.Infrastructure.Repositories;
using EnterpriseEmployeeManagement.Infrastructure.Services;
using EnterpriseEmployeeManagement.WebApi.Controllers;
using EnterpriseEmployeeManagement.WebApi.ExceptionHandling;
using EnterpriseEmployeeManagement.WebApi.HealthChecks;
using EnterpriseEmployeeManagement.WebApi.Middleware;
using EnterpriseEmployeeManagement.WebApi.Options;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;

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
    throw new InvalidOperationException(
        "JWT Key is not configured.");
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
// HEALTH CHECKS
// ============================================================

builder.Services
    .AddHealthChecks()

    // Application is alive
    .AddCheck(
        "self",
        () => HealthCheckResult.Healthy(),
        tags: new[] { "live" })

    // Database is available
    .AddDbContextCheck<ApplicationDbContext>(
        name: "database",
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
// FLUENT VALIDATION
// ============================================================

// builder.Services.AddValidatorsFromAssemblyContaining<
//     CreateEmployeeValidator>();

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
// EMPLOYEE REPOSITORY
// ============================================================

builder.Services.AddScoped<EmployeeRepository>();

// ============================================================
// APPLICATION SERVICES
// ============================================================

builder.Services.AddScoped<
    IEmployeeService,
    EmployeeService>();

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
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    jwtSettings["Issuer"],

                ValidAudience =
                    jwtSettings["Audience"],

                IssuerSigningKey =
                    securityKey,

                ClockSkew =
                    TimeSpan.Zero
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

            Version = "v1"
        });

    // ========================================================
    // JWT SECURITY DEFINITION
    // ========================================================

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
                "Enter your JWT token."
        });

    // ========================================================
    // JWT SECURITY REQUIREMENT
    // ========================================================

    options.AddSecurityRequirement(
        document =>
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecuritySchemeReference(
                        "Bearer",
                        document)
                ] =
                    new List<string>()
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