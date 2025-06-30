using Application.Interfaces;
using Domain.Constants;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class CacheInvalidationService
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheInvalidationService> _logger;

        public CacheInvalidationService(ICacheService cacheService, ILogger<CacheInvalidationService> logger)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void InvalidateSessionCache(string sessionId)
        {
            try
            {
                var cacheKey = CacheKeys.SessionKey(sessionId);
                _cacheService.Remove(cacheKey);
                _logger.LogInformation("Oturum önbelleği geçersiz kılındı. SessionId: {SessionId}", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Oturum önbelleği geçersiz kılınırken hata oluştu. SessionId: {SessionId}", sessionId);
            }
        }

        public void InvalidateLocationCache(string? searchTerm = null)
        {
            try
            {
                var cacheKey = CacheKeys.LocationsSearchKey(searchTerm);
                _cacheService.Remove(cacheKey);
                _logger.LogInformation("Lokasyon önbelleği geçersiz kılındı. Arama terimi: {SearchTerm}", searchTerm ?? "Tümü");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lokasyon önbelleği geçersiz kılınırken hata oluştu. Arama terimi: {SearchTerm}", searchTerm ?? "Tümü");
            }
        }

        public void InvalidateJourneyCache(string originId, string destinationId, DateTime departureDate)
        {
            try
            {
                var cacheKey = CacheKeys.JourneysKey(originId, destinationId, departureDate);
                _cacheService.Remove(cacheKey);
                _logger.LogInformation("Sefer önbelleği geçersiz kılındı. {OriginId} noktasından {DestinationId} noktasına {DepartureDate} tarihinde",
                    originId, destinationId, departureDate.ToString("yyyy-MM-dd"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sefer önbelleği geçersiz kılınırken hata oluştu. {OriginId} noktasından {DestinationId} noktasına {DepartureDate} tarihinde",
                    originId, destinationId, departureDate.ToString("yyyy-MM-dd"));
            }
        }

        public void InvalidateAllLocationCache()
        {
            try
            {
                _cacheService.Remove(CacheKeys.LocationsSearchKey(null));
                _logger.LogInformation("Tüm lokasyon önbellekleri geçersiz kılındı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm lokasyon önbellekleri geçersiz kılınırken hata oluştu");
            }
        }

        public void InvalidateAllJourneyCache()
        {
            try
            {
                _logger.LogInformation("Sefer önbelleği geçersiz kılma isteği alındı. Üretim ortamı için desen tabanlı geçersiz kılma uygulanması düşünülmelidir.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm sefer önbellekleri geçersiz kılınırken hata oluştu");
            }
        }

        public void ClearAllCache()
        {
            try
            {
                _cacheService.Clear();
                _logger.LogInformation("Tüm önbellek temizlendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm önbellek temizlenirken hata oluştu");
            }
        }
    }
} 