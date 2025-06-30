using Application.DTOs;
using Application.DTOs.ExternalApi;
using Application.Interfaces;
using Domain.Exceptions;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Test.Helpers;
using Xunit;

namespace Test.Services
{
    public class ExternalBusApiServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<ILogger<ExternalBusApiService>> _mockLogger;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly ExternalBusApiService _service;
        private readonly HttpClient _httpClient;

        public ExternalBusApiServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockLogger = new Mock<ILogger<ExternalBusApiService>>();
            _mockCacheService = new Mock<ICacheService>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _httpClient.BaseAddress = new Uri("http://localhost");
            _service = new ExternalBusApiService(_httpClient, _mockLogger.Object, _mockCacheService.Object);
        }

        [Fact]
        public async Task GetSessionAsync_WithValidResponse_ReturnsSessionData()
        {
            // Arrange
            var expectedSession = TestDataBuilder.CreateMockSession();
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new SessionResponse
                {
                    Data = new SessionData
                    {
                        SessionId = expectedSession.SessionId,
                        DeviceId = expectedSession.DeviceId
                    }
                }))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _service.GetSessionAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedSession.SessionId, result.SessionId);
            Assert.Equal(expectedSession.DeviceId, result.DeviceId);
        }

        [Fact]
        public async Task GetSessionAsync_WithHttpError_ThrowsExternalApiException()
        {
            // Arrange
            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            // Act & Assert
            await Assert.ThrowsAsync<ExternalApiException>(() => _service.GetSessionAsync());
        }

        [Fact]
        public async Task GetBusLocationsAsync_WithValidResponse_ReturnsLocations()
        {
            // Arrange
            var session = TestDataBuilder.CreateMockSession();
            var expectedLocations = TestDataBuilder.CreateMockLocations();
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new LocationResponse
                {
                    Data = expectedLocations.Select(l => new LocationData
                    {
                        Id = int.Parse(l.Id),
                        Name = l.Name ?? string.Empty,
                        Country = l.Country ?? string.Empty,
                        City = l.City ?? string.Empty
                    }).ToList()
                }))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            _mockCacheService.Setup(x => x.GetAsync<IEnumerable<BusLocationDto>>(It.IsAny<string>())).ReturnsAsync((IEnumerable<BusLocationDto>?)null);
            _mockCacheService.Setup(x => x.Set(It.IsAny<string>(), It.IsAny<IEnumerable<BusLocationDto>>(), It.IsAny<TimeSpan>())).Verifiable();

            // Act
            var result = await _service.GetBusLocationsAsync(session.SessionId, session.DeviceId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedLocations.Count, result.Count());
        }

        [Fact]
        public async Task GetJourneysAsync_WithValidResponse_ReturnsJourneys()
        {
            // Arrange
            var session = TestDataBuilder.CreateMockSession();
            var expectedJourneys = TestDataBuilder.CreateMockJourneys();
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new JourneyResponse
                {
                    Data = expectedJourneys.Select(j => new JourneyData
                    {
                        Id = j.Id,
                        PartnerName = j.Name ?? string.Empty,
                        BusTypeName = j.Description ?? string.Empty,
                        AvailableSeats = j.AvailableSeats,
                        Journey = new JourneyInfo
                        {
                            Origin = j.DepartureLocation ?? string.Empty,
                            Destination = j.Destination ?? string.Empty,
                            Departure = j.DepartureDate.ToString("yyyy-MM-dd HH:mm:ss"),
                            Arrival = j.ReturnDate.ToString("yyyy-MM-dd HH:mm:ss"),
                            OriginalPrice = j.Price
                        }
                    }).ToList()
                }))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _service.GetJourneysAsync(session.SessionId, session.DeviceId, "1", "2", DateTime.Now);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedJourneys.Count, result.Count());
        }

        [Fact]
        public async Task GetJourneysAsync_WithZeroAvailableSeats_FiltersOutJourneys()
        {
            // Arrange
            var session = TestDataBuilder.CreateMockSession();
            var journeysWithZeroSeats = new List<JourneyData>
            {
                new JourneyData
                {
                    Journey = new JourneyInfo
                    {
                        Departure = DateTime.Now.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss"),
                        Arrival = DateTime.Now.AddHours(5).ToString("yyyy-MM-dd HH:mm:ss")
                    }
                }
            };

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new JourneyResponse
                {
                    Data = journeysWithZeroSeats
                }))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _service.GetJourneysAsync(session.SessionId, session.DeviceId, "1", "2", DateTime.Now);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBusLocationsAsync_WithEmptySessionId_ThrowsBusTourException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<BusTourException>(() => 
                _service.GetBusLocationsAsync("", "device-id"));
        }

        [Fact]
        public async Task GetJourneysAsync_WithEmptySessionId_ThrowsBusTourException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<BusTourException>(() => 
                _service.GetJourneysAsync("", "device-id", "1", "2", DateTime.Now));
        }
    }
} 