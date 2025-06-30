using Microsoft.Extensions.Diagnostics.HealthChecks;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Web.Services.HealthChecks
{
    public class ExternalApiHealthCheck : IHealthCheck
    {
        private readonly IExternalBusApiService _externalBusApiService;
        private readonly ILogger<ExternalApiHealthCheck> _logger;

        public ExternalApiHealthCheck(IExternalBusApiService externalBusApiService, ILogger<ExternalApiHealthCheck> logger)
        {
            _externalBusApiService = externalBusApiService;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Harici API sağlık kontrolü başlatılıyor");

                var session = await _externalBusApiService.GetSessionAsync();
                
                if (string.IsNullOrEmpty(session.SessionId) || string.IsNullOrEmpty(session.DeviceId))
                {
                    _logger.LogWarning("Geçersiz oturum verisi alındı");
                    return HealthCheckResult.Degraded("Geçersiz oturum verisi");
                }

                _logger.LogInformation("Harici API sağlık kontrolü başarılı. SessionId: {SessionId}", session.SessionId);
                return HealthCheckResult.Healthy("Harici API erişilebilir");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Harici API sağlık kontrolü başarısız");
                return HealthCheckResult.Unhealthy("Harici API erişilemiyor", ex);
            }
        }
    }
} 