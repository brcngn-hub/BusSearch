using Application.DTOs;
using Domain.Exceptions;
using Application.Interfaces;
using Application.DTOs.ExternalApi;
using Domain.Constants;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System.Net;
using System.Text;

namespace Infrastructure.Services
{
    public class ExternalBusApiService : IExternalBusApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExternalBusApiService> _logger;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly ICacheService _cacheService;

        public ExternalBusApiService(
            HttpClient httpClient, 
            ILogger<ExternalBusApiService> logger,
            ICacheService cacheService)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));

            _retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(3, retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timeSpan, retryCount, context) =>
                    {
                        var errorMsg = outcome.Exception != null ? outcome.Exception.Message : outcome.Result?.StatusCode.ToString();
                        _logger.LogWarning("Yeniden deneme girişimi {RetryAttempt} {Delay}ms sonra {Error} hatası nedeniyle",
                            retryCount, timeSpan.TotalMilliseconds, errorMsg);
                    });
        }

        public async Task<SessionDto> GetSessionAsync()
        {
            try
            {
                _logger.LogInformation("Harici API ile oturum oluşturuluyor");

                var request = new GetSessionRequest
                {
                    Connection = new GetSessionConnection
                    {
                        IpAddress = "127.0.0.1",
                        Port = "5117"
                    },
                    Browser = new GetSessionBrowser
                    {
                        Name = "Chrome",
                        Version = "47.0.0.12"
                    }
                };

                var jsonContent = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _retryPolicy.ExecuteAsync(async () =>
                    await _httpClient.PostAsync("/api/client/getsession", content));

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Oturum alınamadı. Durum: {StatusCode}", response.StatusCode);
                    throw new ExternalApiException($"Oturum oluşturulamadı. HTTP {response.StatusCode}", (int)response.StatusCode);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Oturum API Yanıtı: {ResponseContent}", responseContent);
                
                var sessionResponse = JsonConvert.DeserializeObject<SessionResponse>(responseContent);

                if (sessionResponse?.Data == null)
                {
                    _logger.LogError("Geçersiz oturum yanıt formatı. Yanıt: {ResponseContent}", responseContent);
                    throw new ExternalApiException("Geçersiz oturum yanıt formatı", 500);
                }

                var sessionData = new SessionDto(sessionResponse.Data.SessionId, sessionResponse.Data.DeviceId);
                _cacheService.Set(CacheKeys.SessionKey(sessionResponse.Data.SessionId), sessionData, CacheKeys.Expiration.Session);

                _logger.LogInformation("Oturum başarıyla oluşturuldu. SessionId: {SessionId}", sessionResponse.Data.SessionId);
                return sessionData;
            }
            catch (ExternalApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Oturum oluşturma sırasında beklenmeyen hata");
                throw new ExternalApiException("Oturum oluşturulurken beklenmeyen hata oluştu", 500);
            }
        }

        public async Task<IEnumerable<BusLocationDto>> GetBusLocationsAsync(string sessionId, string deviceId)
        {
            return await GetBusLocationsAsync(sessionId, deviceId, null);
        }

        public async Task<IEnumerable<BusLocationDto>> GetBusLocationsAsync(string sessionId, string deviceId, string? searchTerm = null)
        {
            try
            {
                ValidateSessionParameters(sessionId, deviceId);

                // Try to get from cache first
                var cacheKey = CacheKeys.LocationsSearchKey(searchTerm);
                var cachedLocations = await _cacheService.GetAsync<IEnumerable<BusLocationDto>>(cacheKey);
                if (cachedLocations != null)
                {
                    _logger.LogInformation("Önbellekten {Count} lokasyon alındı. Arama Terimi: {SearchTerm}",
                        cachedLocations.Count(), searchTerm ?? "Tümü");
                    return cachedLocations;
                }

                _logger.LogInformation("API'den otobüs lokasyonları alınıyor. Arama Terimi: {SearchTerm}", searchTerm ?? "Tümü");

                var request = new GetBusLocationsRequest
                {
                    Data = searchTerm ?? "",
                    DeviceSession = new SessionData
                    {
                        SessionId = sessionId,
                        DeviceId = deviceId
                    },
                    Date = DateTime.Now.ToString("yyyy-MM-dd"),
                    Language = "tr-TR"
                };

                var jsonContent = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _retryPolicy.ExecuteAsync(async () =>
                    await _httpClient.PostAsync("/api/location/getbuslocations", content));

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Otobüs lokasyonları alınamadı. Durum: {StatusCode}", response.StatusCode);
                    throw new ExternalApiException($"Otobüs lokasyonları alınamadı. Durum: {response.StatusCode}", (int)response.StatusCode);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var locationResponse = JsonConvert.DeserializeObject<LocationResponse>(responseContent);

                if (locationResponse?.Data == null)
                {
                    _logger.LogWarning("Lokasyon bulunamadı veya geçersiz yanıt formatı");
                    return new List<BusLocationDto>();
                }

                var locations = locationResponse.Data.Select(l => new BusLocationDto
                {
                    Id = l.Id.ToString(),
                    Name = l.Name,
                    Country = l.Country,
                    City = l.City
                }).ToList();

                // Cache the locations
                _cacheService.Set(cacheKey, locations, CacheKeys.Expiration.Locations);

                _logger.LogInformation("API'den {Count} lokasyon alındı ve önbelleğe kaydedildi", locations.Count);
                return locations;
            }
            catch (ExternalApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lokasyon arama sırasında beklenmeyen hata");
                throw new BusTourException("Otobüs lokasyonları alınamadı", ex, "EXTERNAL_API_ERROR");
            }
        }

        public async Task<IEnumerable<BusTourDto>> GetJourneysAsync(string sessionId, string deviceId, string originId, string destinationId, DateTime departureDate)
        {
            try
            {
                ValidateSessionParameters(sessionId, deviceId);
                ValidateJourneyParameters(originId, destinationId, departureDate);

                _logger.LogInformation("API'den seferler alınıyor. {OriginId} noktasından {DestinationId} noktasına {DepartureDate} tarihinde",
                    originId, destinationId, departureDate.ToString("yyyy-MM-dd"));

                // originId ve destinationId int olmalı
                if (!int.TryParse(originId, out var originIdInt))
                    throw new ArgumentException("Kalkış noktası ID'si sayı olmalıdır", nameof(originId));
                if (!int.TryParse(destinationId, out var destinationIdInt))
                    throw new ArgumentException("Varış noktası ID'si sayı olmalıdır", nameof(destinationId));

                var request = new GetJourneysRequest
                {
                    Data = new GetJourneysData
                    {
                        OriginId = originIdInt,
                        DestinationId = destinationIdInt,
                        DepartureDate = departureDate.ToString("yyyy-MM-dd")
                    },
                    DeviceSession = new SessionData
                    {
                        SessionId = sessionId,
                        DeviceId = deviceId
                    },
                    Date = DateTime.Now.ToString("yyyy-MM-dd"),
                    Language = "tr-TR"
                };

                var jsonContent = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _retryPolicy.ExecuteAsync(async () =>
                    await _httpClient.PostAsync("/api/journey/getbusjourneys", content));

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Seferler alınamadı. Durum: {StatusCode}", response.StatusCode);
                    throw new ExternalApiException($"Seferler alınamadı. Durum: {response.StatusCode}", (int)response.StatusCode);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var journeyResponse = JsonConvert.DeserializeObject<JourneyResponse>(responseContent);

                if (journeyResponse?.Data == null)
                {
                    _logger.LogWarning("Sefer bulunamadı veya geçersiz yanıt formatı");
                    return new List<BusTourDto>();
                }

                var journeys = journeyResponse.Data.Select(j => new BusTourDto
                {
                    Id = j.Id,
                    Name = j.PartnerName,
                    Description = j.BusTypeName,
                    DepartureLocation = j.Journey.Origin,
                    Destination = j.Journey.Destination,
                    DepartureDate = DateTime.TryParse(j.Journey.Departure, out var dep) ? dep : DateTime.MinValue,
                    ReturnDate = DateTime.TryParse(j.Journey.Arrival, out var arr) ? arr : DateTime.MinValue,
                    Price = j.Journey.OriginalPrice,
                    AvailableSeats = j.AvailableSeats,
                    IsActive = true, 
                    CreatedAt = DateTime.Now, 
                    UpdatedAt = null, 
                    Stops = j.Journey.Stops?.Select(s => new BusStopDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Station = s.Station,
                        Time = DateTime.TryParse(s.Time, out var t) ? t : (DateTime?)null,
                        IsOrigin = s.IsOrigin,
                        IsDestination = s.IsDestination,
                        Index = s.Index
                    }).ToList(),
                    Features = j.Journey.Features
                }).Where(j => j.AvailableSeats > 0).ToList();

                _logger.LogInformation("API'den {Count} sefer alındı (sadece boş koltuklu olanlar) ve önbelleğe kaydedildi", journeys.Count);
                return journeys;
            }
            catch (ExternalApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sefer arama sırasında beklenmeyen hata");
                throw new BusTourException("Seferler alınamadı", ex, "EXTERNAL_API_ERROR");
            }
        }

        private void ValidateSessionParameters(string sessionId, string deviceId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Oturum ID'si boş olamaz", nameof(sessionId));
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("Cihaz ID'si boş olamaz", nameof(deviceId));
        }

        private void ValidateJourneyParameters(string originId, string destinationId, DateTime departureDate)
        {
            if (string.IsNullOrWhiteSpace(originId))
                throw new ArgumentException("Kalkış noktası ID'si boş olamaz", nameof(originId));
            if (string.IsNullOrWhiteSpace(destinationId))
                throw new ArgumentException("Varış noktası ID'si boş olamaz", nameof(destinationId));
            if (departureDate < DateTime.Today)
                throw new ArgumentException("Kalkış tarihi geçmiş bir tarih olamaz", nameof(departureDate));
        }
    }
} 