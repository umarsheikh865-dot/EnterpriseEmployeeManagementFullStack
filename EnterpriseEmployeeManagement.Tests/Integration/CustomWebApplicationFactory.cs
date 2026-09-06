using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EnterpriseEmployeeManagement.Tests.Integration
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.EntityFrameworkCore;
    using EnterpriseEmployeeManagement.Infrastructure.Persistence;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public class CustomWebApplicationFactory
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(
            IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            // Replace ApplicationDbContext registration with InMemory for tests.
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registrations
                services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
                services.RemoveAll(typeof(ApplicationDbContext));

                // Add in-memory ApplicationDbContext using an isolated EF service provider
                var efProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestIntegrationDb");
                    options.UseInternalServiceProvider(efProvider);
                });

                // Do not modify health check registrations here; Program.cs
                // registers health checks and will use the in-memory
                // ApplicationDbContextHealthCheck we added earlier.
            });
        }
    }
}