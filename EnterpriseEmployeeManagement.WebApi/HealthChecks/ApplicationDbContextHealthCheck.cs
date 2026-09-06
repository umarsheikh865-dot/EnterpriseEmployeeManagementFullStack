using EnterpriseEmployeeManagement.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseEmployeeManagement.WebApi.HealthChecks
{
    public class ApplicationDbContextHealthCheck : IHealthCheck
    {
        private readonly ApplicationDbContext _db;

        public ApplicationDbContextHealthCheck(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // EF InMemory provider does not support CanConnect; treat as healthy
                var provider = _db.Database.ProviderName ?? string.Empty;
                if (provider.Contains("InMemory", StringComparison.OrdinalIgnoreCase))
                {
                    return HealthCheckResult.Healthy();
                }

                var canConnect = await _db.Database.CanConnectAsync(cancellationToken);

                return canConnect
                    ? HealthCheckResult.Healthy()
                    : HealthCheckResult.Unhealthy("Database cannot connect");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(ex.Message);
            }
        }
    }
}
