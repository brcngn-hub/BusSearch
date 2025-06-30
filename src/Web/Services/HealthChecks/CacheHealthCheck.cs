using Microsoft.Extensions.Diagnostics.HealthChecks;
using Application.Interfaces;

namespace Web.Services.HealthChecks
{
    public class CacheHealthCheck : IHealthCheck
    {
        private readonly ICacheService _cacheService;

        public CacheHealthCheck(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var testKey = "health_check_test";
                var testValue = DateTime.UtcNow.ToString();
                _cacheService.Set(testKey, testValue, TimeSpan.FromMinutes(1));
                var retrievedValue = _cacheService.Get<string>(testKey);
                if (retrievedValue == testValue)
                {
                    return Task.FromResult(HealthCheckResult.Healthy("Önbellek düzgün çalışıyor"));
                }
                else
                {
                    return Task.FromResult(HealthCheckResult.Degraded("Önbellek okuma/yazma uyumsuzluğu"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Önbellek sağlık kontrolü başarısız", ex));
            }
        }
    }
} 