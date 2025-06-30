using Application.Interfaces;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class BusToursController : Controller
    {
        private readonly IBusTourService _busTourService;
        private readonly ILogger<BusToursController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SessionKey = "ObiletSessionId";
        private const string DeviceKey = "ObiletDeviceId";

        public BusToursController(
            IBusTourService busTourService, 
            ILogger<BusToursController> logger, 
            IHttpContextAccessor httpContextAccessor)
        {
            _busTourService = busTourService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<(string SessionId, string DeviceId)> GetOrCreateUserSessionAsync()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                _logger.LogError("Oturum mevcut değil");
                throw new InvalidOperationException("Oturum mevcut değil");
            }

            var sessionId = session.GetString(SessionKey);
            var deviceId = session.GetString(DeviceKey);

            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(deviceId))
            {
                _logger.LogInformation("Kullanıcı için yeni oturum oluşturuluyor");
                var sessionPair = await _busTourService.CreateObiletSessionAsync();
                sessionId = sessionPair.SessionId;
                deviceId = sessionPair.DeviceId;
                session.SetString(SessionKey, sessionId);
                session.SetString(DeviceKey, deviceId);
                _logger.LogInformation("Oturum oluşturuldu ve saklandı. SessionId: {SessionId}", sessionId);
            }
            return (sessionId, deviceId);
        }

        public async Task<IActionResult> Index(string? search = null)
        {
            try
            {
                _logger.LogInformation("Index action çağrıldı. Arama: {Search}", search ?? "null");
                var (sessionId, deviceId) = await GetOrCreateUserSessionAsync();
                var locations = await _busTourService.GetBusLocationsAsync(sessionId, deviceId, search);

                _logger.LogInformation("'{Search}' araması için filtrelenmiş lokasyonlar. {Count} sonuç bulundu", search, locations.Count());

                ViewBag.SearchTerm = search;
                ViewBag.Locations = locations;
                return View();
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Index action'da doğrulama hatası: {Message}", ex.Message);
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
            catch (ExternalApiException ex)
            {
                var statusCode = ex.StatusCode;
                _logger.LogError(ex, "Index action'da harici API hatası. Durum Kodu: {StatusCode}", statusCode);
                ViewBag.ErrorMessage = $"Harici servis hatası: {ex.Message}";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Index action'da beklenmeyen hata");
                ViewBag.ErrorMessage = "Beklenmeyen bir hata oluştu";
                return View();
            }
        }

        public async Task<IActionResult> BusTours(string originId, string destinationId, DateTime departureDate, string? sort = null)
        {
            try
            {
                _logger.LogInformation("BusTours action çağrıldı. Başlangıç: {OriginId}, Hedef: {DestinationId}, Tarih: {DepartureDate}, Sıralama: {Sort}",
                    originId, destinationId, departureDate.ToString("yyyy-MM-dd"), sort ?? "varsayılan");

                if (string.IsNullOrEmpty(originId) || string.IsNullOrEmpty(destinationId))
                {
                    _logger.LogWarning("Gerekli parametreler eksik. OriginId: {OriginId}, DestinationId: {DestinationId}", originId, destinationId);
                    ViewBag.ErrorMessage = "Başlangıç ve hedef noktaları seçilmelidir";
                    return View("Index");
                }

                var (sessionId, deviceId) = await GetOrCreateUserSessionAsync();
                var journeys = await _busTourService.GetJourneysAsync(originId, destinationId, departureDate, sessionId, deviceId);
                
                // Apply sorting
                if (string.IsNullOrEmpty(sort) || sort == "departureAsc")
                {
                    journeys = journeys.OrderBy(j => j.DepartureDate).ToList();
                    sort = "departureAsc";
                }
                else
                {
                    switch (sort)
                    {
                        case "departureDesc":
                            journeys = journeys.OrderByDescending(j => j.DepartureDate).ToList();
                            break;
                        case "priceAsc":
                            journeys = journeys.OrderBy(j => j.Price).ToList();
                            break;
                        case "priceDesc":
                            journeys = journeys.OrderByDescending(j => j.Price).ToList();
                            break;
                    }
                }

                var locations = await _busTourService.GetBusLocationsAsync(sessionId, deviceId);
                var origin = locations.FirstOrDefault(l => l.Id == originId);
                var destination = locations.FirstOrDefault(l => l.Id == destinationId);

                _logger.LogInformation("Sefer araması tamamlandı. {Count} sefer bulundu", journeys.Count());

                ViewBag.Journeys = journeys;
                ViewBag.OriginId = originId;
                ViewBag.DestinationId = destinationId;
                ViewBag.OriginName = origin?.Name ?? originId;
                ViewBag.DestinationName = destination?.Name ?? destinationId;
                ViewBag.DepartureDate = departureDate;
                ViewBag.Sort = sort;
                return View();
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("BusTours action'da doğrulama hatası: {Message}", ex.Message);
                ViewBag.ErrorMessage = ex.Message;
                return View("Index");
            }
            catch (ExternalApiException ex)
            {
                var statusCode = ex.StatusCode;
                _logger.LogError(ex, "BusTours action'da harici API hatası. Durum Kodu: {StatusCode}", statusCode);
                ViewBag.ErrorMessage = $"Harici servis hatası: {ex.Message}";
                return View("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BusTours action'da beklenmeyen hata");
                ViewBag.ErrorMessage = "Beklenmeyen bir hata oluştu";
                return View("Index");
            }
        }
    }
} 