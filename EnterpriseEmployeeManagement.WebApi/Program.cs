using EnterpriseEmployeeManagement.Application.Interfaces;
using EnterpriseEmployeeManagement.Application.Interfaces.Repositories;
using EnterpriseEmployeeManagement.Application.Interfaces.Services;
using EnterpriseEmployeeManagement.Infrastructure.DependencyInjection;
using EnterpriseEmployeeManagement.Infrastructure.Repositories;
using EnterpriseEmployeeManagement.Infrastructure.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// JWT CONFIGURATION
// ============================================

var jwtSettings = builder.Configuration.GetSection("Jwt");

var jwtKey = jwtSettings["Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JWT Key is not configured.");
}


// ============================================
// JWT KEY
// ============================================

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

if (keyBytes.Length < 32)
{
    keyBytes = SHA256.HashData(keyBytes);
}

var securityKey = new SymmetricSecurityKey(keyBytes);


// ============================================
// CONTROLLERS
// ============================================

builder.Services.AddControllers();


// ============================================
// INFRASTRUCTURE
// ============================================

builder.Services.AddInfrastructure(
    builder.Configuration);


// ============================================
// EMPLOYEE REPOSITORY
// ============================================

builder.Services.AddScoped<EmployeeRepository>();


// ============================================
// APPLICATION SERVICES
// ============================================

builder.Services.AddScoped<
    IEmployeeService,
    EmployeeService>();


// ============================================
// JWT AUTHENTICATION
// ============================================

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

                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],

                IssuerSigningKey = securityKey,

                ClockSkew = TimeSpan.Zero
            };
    });


// ============================================
// AUTHORIZATION
// ============================================

builder.Services.AddAuthorization();


// ============================================
// SWAGGER
// ============================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "EnterpriseEmployeeManagement.WebApi",
            Version = "v1"
        });


    // ========================================
    // JWT SECURITY DEFINITION
    // ========================================

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type = SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT",

            In = ParameterLocation.Header,

            Description = "Enter your JWT token."
        });


    // ========================================
    // JWT SECURITY REQUIREMENT
    // ========================================

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


// ============================================
// BUILD APPLICATION
// ============================================

var app = builder.Build();


// ============================================
// GLOBAL EXCEPTION HANDLING
// ============================================

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception =
            context.Features
                .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()
                ?.Error;

        context.Response.ContentType = "application/json";


        // ----------------------------------------
        // UNAUTHORIZED
        // ----------------------------------------

        if (exception is UnauthorizedAccessException)
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsJsonAsync(
                new
                {
                    message = exception.Message
                });

            return;
        }


        // ----------------------------------------
        // BAD REQUEST
        // ----------------------------------------

        if (exception is ArgumentException)
        {
            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(
                new
                {
                    message = exception.Message
                });

            return;
        }


        // ----------------------------------------
        // CONFLICT
        // ----------------------------------------

        if (exception is InvalidOperationException)
        {
            context.Response.StatusCode =
                StatusCodes.Status409Conflict;

            await context.Response.WriteAsJsonAsync(
                new
                {
                    message = exception.Message
                });

            return;
        }


        // ----------------------------------------
        // INTERNAL SERVER ERROR
        // ----------------------------------------

        context.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(
            new
            {
                message = "An unexpected error occurred."
            });
    });
});


// ============================================
// SWAGGER
// ============================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// ============================================
// MIDDLEWARE
// ============================================

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();


// ============================================
// CONTROLLERS
// ============================================

app.MapControllers();

app.Run();