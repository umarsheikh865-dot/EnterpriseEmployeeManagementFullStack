using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace EnterpriseEmployeeManagement.WebApi.HealthChecks
{
    public static class HealthCheckResponseWriter
    {
        public static async Task WriteResponse(
            HttpContext context,
            HealthReport report)
        {
            context.Response.ContentType =
                "application/json";

            var response = new
            {
                status =
                    report.Status.ToString(),

                totalDuration =
                    report.TotalDuration.TotalMilliseconds,

                checks =
                    report.Entries.Select(entry => new
                    {
                        name = entry.Key,

                        status =
                            entry.Value.Status.ToString(),

                        duration =
                            entry.Value.Duration
                                .TotalMilliseconds,

                        error =
                            entry.Value.Exception?.Message
                    })
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}