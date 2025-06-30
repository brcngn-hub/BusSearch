using Application.DTOs;
using Application.Interfaces;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class BusTourService : IBusTourService
    {
        private readonly IExternalBusApiService _externalBusApiService;
        private readonly ILogger<BusTourService> _logger;

        public BusTourService(
            IExternalBusApiService externalBusApiService,
            ILogger<BusTourService> logger)
        {
            _externalBusApiService = externalBusApiService;
            _logger = logger;
        }

        public async Task<SessionDto> CreateObiletSessionAsync()
        {
            _logger.LogInformation("Obilet oturumu oluşturuluyor");
            return await _externalBusApiService.GetSessionAsync();
        }

        public async Task<IEnumerable<BusLocationDto>> GetBusLocationsAsync(string sessionId, string deviceId)
        {
            return await GetBusLocationsAsync(sessionId, deviceId, null);
        }

        public async Task<IEnumerable<BusLocationDto>> GetBusLocationsAsync(string sessionId, string deviceId, string? searchTerm)
        {
            try
            {
                if (string.IsNullOrEmpty(sessionId))
                    throw new ValidationException("Oturum ID'si boş olamaz");
                if (string.IsNullOrEmpty(deviceId))
                    throw new ValidationException("Cihaz ID'si boş olamaz");

                _logger.LogInformation("Otobüs lokasyonları getiriliyor. Arama Terimi: {SearchTerm}", searchTerm ?? "Tümü");
                
                var locations = await _externalBusApiService.GetBusLocationsAsync(sessionId, deviceId, searchTerm);
                
                _logger.LogInformation("Başarıyla {Count} otobüs lokasyonu alındı", locations.Count());
                return locations;
            }
            catch (ValidationException)
            {
                _logger.LogWarning("Otobüs lokasyonları getirme sırasında doğrulama hatası");
                throw;
            }
            catch (ExternalApiException ex)
            {
                _logger.LogError(ex, "Otobüs lokasyonları getirme sırasında harici API hatası. Durum Kodu: {StatusCode}", ex.StatusCode);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otobüs lokasyonları getirme sırasında beklenmeyen hata");
                throw new BusTourException("Otobüs lokasyonları alınamadı", ex, "LOCATIONS_FETCH_ERROR");
            }
        }

        public async Task<IEnumerable<BusTourDto>> GetJourneysAsync(string originId, string destinationId, DateTime departureDate, string sessionId, string deviceId)
        {
            try
            {
                if (string.IsNullOrEmpty(originId))
                    throw new ValidationException("Kalkış noktası ID'si boş olamaz");
                if (string.IsNullOrEmpty(destinationId))
                    throw new ValidationException("Varış noktası ID'si boş olamaz");
                if (string.IsNullOrEmpty(sessionId))
                    throw new ValidationException("Oturum ID'si boş olamaz");
                if (string.IsNullOrEmpty(deviceId))
                    throw new ValidationException("Cihaz ID'si boş olamaz");
                if (departureDate.Date < DateTime.Today)
                    throw new ValidationException("Kalkış tarihi geçmiş bir tarih olamaz");

                _logger.LogInformation("{OriginId} noktasından {DestinationId} noktasına {DepartureDate} tarihinde sefer aranıyor", 
                    originId, destinationId, departureDate.ToString("yyyy-MM-dd"));
                
                var journeys = await _externalBusApiService.GetJourneysAsync(sessionId, deviceId, originId, destinationId, departureDate);
                
                _logger.LogInformation("Başarıyla {Count} sefer alındı", journeys.Count());
                return journeys;
            }
            catch (ValidationException)
            {
                _logger.LogWarning("Sefer arama sırasında doğrulama hatası");
                throw;
            }
            catch (ExternalApiException ex)
            {
                _logger.LogError(ex, "Sefer arama sırasında harici API hatası. Durum Kodu: {StatusCode}", ex.StatusCode);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sefer arama sırasında beklenmeyen hata");
                throw new BusTourException("Seferler alınamadı", ex, "JOURNEY_SEARCH_ERROR");
            }
        }
    }
} 