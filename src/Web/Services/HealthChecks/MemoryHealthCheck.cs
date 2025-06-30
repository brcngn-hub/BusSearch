using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Web.Services.HealthChecks
{
    public class MemoryHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var memoryInfo = GC.GetGCMemoryInfo();
            var totalMemory = GC.GetTotalMemory(false);
            var maxMemory = memoryInfo.TotalAvailableMemoryBytes;
            var memoryUsagePercentage = (double)totalMemory / maxMemory * 100;

            if (memoryUsagePercentage < 80)
            {
                return Task.FromResult(HealthCheckResult.Healthy($"Bellek kullanımı normal: {memoryUsagePercentage:F2}%"));
            }
            else if (memoryUsagePercentage < 95)
            {
                return Task.FromResult(HealthCheckResult.Degraded($"Bellek kullanımı yüksek: {memoryUsagePercentage:F2}%"));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy($"Bellek kullanımı kritik: {memoryUsagePercentage:F2}%"));
            }
        }
    }
} 