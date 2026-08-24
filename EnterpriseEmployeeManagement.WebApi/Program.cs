using EnterpriseEmployeeManagement.WebApi.Options;
using EnterpriseEmployeeManagement.WebApi.Middleware;
using EnterpriseEmployeeManagement.WebApi.ExceptionHandling;

using EnterpriseEmployeeManagement.Application.Interfaces;
using EnterpriseEmployeeManagement.Application.Interfaces.Repositories;
using EnterpriseEmployeeManagement.Application.Interfaces.Services;
using EnterpriseEmployeeManagement.Application.Validators;

using EnterpriseEmployeeManagement.Infrastructure.DependencyInjection;
using EnterpriseEmployeeManagement.Infrastructure.Repositories;
using EnterpriseEmployeeManagement.Infrastructure.Services;

using FluentValidation;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// ============================================================
// JWT CONFIGURATION
// ============================================================

var jwtSettings =
    builder.Configuration.GetSection("JwtSettings");

var jwtKey = jwtSettings["Key"];

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
// PROBLEM DETAILS
// ============================================================

builder.Services.AddProblemDetails();


// ============================================================
// FLUENTVALIDATION
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

builder.Services.AddAuthorization();


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
                ] = new List<string>()
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
// AUTHENTICATION
// ============================================================

app.UseAuthentication();


// ============================================================
// AUTHORIZATION
// ============================================================

app.UseAuthorization();


// ============================================================
// CONTROLLERS
// ============================================================

app.MapControllers();


// ============================================================
// RUN APPLICATION
// ============================================================

app.Run();