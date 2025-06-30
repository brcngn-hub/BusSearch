using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CacheController : ControllerBase
    {
        private readonly CacheInvalidationService _cacheInvalidationService;
        private readonly ILogger<CacheController> _logger;

        public CacheController(CacheInvalidationService cacheInvalidationService, ILogger<CacheController> logger)
        {
            _cacheInvalidationService = cacheInvalidationService ?? throw new ArgumentNullException(nameof(cacheInvalidationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("invalidate/session/{sessionId}")]
        public IActionResult InvalidateSessionCache(string sessionId)
        {
            try
            {
                _cacheInvalidationService.InvalidateSessionCache(sessionId);
                return Ok(new { message = $"Oturum önbelleği geçersiz kılındı: {sessionId}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Oturum önbelleği geçersiz kılınırken hata oluştu");
                return StatusCode(500, new { error = "Oturum önbelleği geçersiz kılınamadı" });
            }
        }

        [HttpPost("invalidate/locations")]
        public IActionResult InvalidateLocationCache([FromQuery] string? searchTerm = null)
        {
            try
            {
                _cacheInvalidationService.InvalidateLocationCache(searchTerm);
                return Ok(new { message = $"Lokasyon önbelleği arama terimi için geçersiz kılındı: {searchTerm ?? "Tümü"}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lokasyon önbelleği geçersiz kılınırken hata oluştu");
                return StatusCode(500, new { error = "Lokasyon önbelleği geçersiz kılınamadı" });
            }
        }

        [HttpPost("invalidate/journeys")]
        public IActionResult InvalidateJourneyCache([FromQuery] string originId, [FromQuery] string destinationId, [FromQuery] DateTime departureDate)
        {
            try
            {
                _cacheInvalidationService.InvalidateJourneyCache(originId, destinationId, departureDate);
                return Ok(new { message = $"Sefer önbelleği geçersiz kılındı: {originId} noktasından {destinationId} noktasına {departureDate:yyyy-MM-dd} tarihinde" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sefer önbelleği geçersiz kılınırken hata oluştu");
                return StatusCode(500, new { error = "Sefer önbelleği geçersiz kılınamadı" });
            }
        }

        [HttpPost("invalidate/all-locations")]
        public IActionResult InvalidateAllLocationCache()
        {
            try
            {
                _cacheInvalidationService.InvalidateAllLocationCache();
                return Ok(new { message = "Tüm lokasyon önbellekleri geçersiz kılındı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm lokasyon önbellekleri geçersiz kılınırken hata oluştu");
                return StatusCode(500, new { error = "Tüm lokasyon önbellekleri geçersiz kılınamadı" });
            }
        }

        [HttpPost("invalidate/all-journeys")]
        public IActionResult InvalidateAllJourneyCache()
        {
            try
            {
                _cacheInvalidationService.InvalidateAllJourneyCache();
                return Ok(new { message = "Tüm sefer önbellekleri geçersiz kılındı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm sefer önbellekleri geçersiz kılınırken hata oluştu");
                return StatusCode(500, new { error = "Tüm sefer önbellekleri geçersiz kılınamadı" });
            }
        }

        [HttpPost("clear")]
        public IActionResult ClearAllCache()
        {
            try
            {
                _cacheInvalidationService.ClearAllCache();
                return Ok(new { message = "Tüm önbellek temizlendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm önbellek temizlenirken hata oluştu");
                return StatusCode(500, new { error = "Tüm önbellek temizlenemedi" });
            }
        }

        [HttpGet("status")]
        public IActionResult GetCacheStatus()
        {
            try
            {
                var status = new
                {
                    timestamp = DateTime.UtcNow,
                    message = "Önbellek servisi çalışıyor",
                    cacheTypes = new[]
                    {
                        new { type = "Oturum", expiration = "1 saat" },
                        new { type = "Lokasyonlar", expiration = "30 dakika" },
                        new { type = "Seferler", expiration = "15 dakika" }
                    }
                };

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Önbellek durumu alınırken hata oluştu");
                return StatusCode(500, new { error = "Önbellek durumu alınamadı" });
            }
        }
    }
} 