using EnterpriseEmployeeManagement.Application.DTOs.Authentication;
using EnterpriseEmployeeManagement.Application.Interfaces;
using EnterpriseEmployeeManagement.Application.Interfaces.Repositories;
using EnterpriseEmployeeManagement.Application.Interfaces.Services;

using EnterpriseEmployeeManagement.Infrastructure.Persistence;
using EnterpriseEmployeeManagement.Infrastructure.Repositories;
using EnterpriseEmployeeManagement.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseEmployeeManagement.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ============================================
            // EF CORE
            // ============================================

            // Allow tests to override database provider using configuration
            var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase");

            if (useInMemory)
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestIntegrationDb");
                });
            }
            else
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(
                        configuration.GetConnectionString(
                            "DefaultConnection"));
                });
            }

            // ============================================
            // GENERIC REPOSITORY
            // ============================================

            services.AddScoped(
                typeof(IGenericRepository<>),
                typeof(GenericRepository<>));

            // ============================================
            // EMPLOYEE REPOSITORY
            // ============================================

            services.AddScoped<
                IEmployeeRepository,
                EmployeeRepository>();

            // ============================================
            // UNIT OF WORK
            // ============================================

            services.AddScoped<
                IUnitOfWork,
                UnitOfWork>();

            // ============================================
            // JWT SETTINGS
            // ============================================

            services.Configure<JwtSettings>(
                configuration.GetSection("JwtSettings"));

            // ============================================
            // PASSWORD SERVICE
            // ============================================

            services.AddScoped<
                IPasswordService,
                PasswordService>();

            // ============================================
            // JWT SERVICE
            // ============================================

            services.AddScoped<
                IJwtService,
                JwtService>();

            // ============================================
            // AUTHENTICATION SERVICE
            // ============================================

            services.AddScoped<
                IAuthenticationService,
                AuthenticationService>();

            // ============================================
            // EMPLOYEE SERVICE
            // ============================================

            services.AddScoped<
                IEmployeeService,
                EmployeeService>();

            // ============================================
            // KEYED EMPLOYEE NOTIFICATION SERVICES
            // ============================================

            services.AddKeyedScoped<
                IEmployeeNotificationService,
                EmailNotificationService>("email");

            services.AddKeyedScoped<
                IEmployeeNotificationService,
                SmsNotificationService>("sms");

            // ============================================
            // RETURN SERVICES
            // ============================================

            return services;
        }
    }
}