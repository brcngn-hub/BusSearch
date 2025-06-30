using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Test.Helpers;
using Xunit;

namespace Test.Services
{
    public class BusTourServiceTests
    {
        private readonly Mock<IExternalBusApiService> _mockExternalApi;
        private readonly Mock<ILogger<BusTourService>> _mockLogger;
        private readonly BusTourService _busTourService;

        public BusTourServiceTests()
        {
            _mockExternalApi = new Mock<IExternalBusApiService>();
            _mockLogger = new Mock<ILogger<BusTourService>>();
            _busTourService = new BusTourService(_mockExternalApi.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetJourneysAsync_WithValidParameters_ReturnsJourneys()
        {
            // Arrange
            var session = TestDataBuilder.CreateMockSession();
            var expectedJourneys = TestDataBuilder.CreateMockJourneys();
            
            _mockExternalApi.Setup(x => x.GetSessionAsync())
                .ReturnsAsync(session);
            _mockExternalApi.Setup(x => x.GetJourneysAsync(session.SessionId, session.DeviceId, "1", "2", It.IsAny<DateTime>()))
                .ReturnsAsync(expectedJourneys);

            // Act
            var result = await _busTourService.GetJourneysAsync("1", "2", DateTime.Now, session.SessionId, session.DeviceId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedJourneys.Count, result.Count());
        }

        [Fact]
        public async Task GetJourneysAsync_WithInvalidParameters_ThrowsValidationException()
        {
            // Arrange
            var originId = "";
            var destinationId = "2";
            var departureDate = DateTime.Today.AddDays(1);
            var sessionId = "test-session-id";
            var deviceId = "test-device-id";

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _busTourService.GetJourneysAsync(originId, destinationId, departureDate, sessionId, deviceId));
        }

        [Fact]
        public async Task CreateObiletSessionAsync_ShouldReturnSessionData()
        {
            // Arrange
            var expectedSession = TestDataBuilder.CreateMockSession();
            _mockExternalApi.Setup(x => x.GetSessionAsync())
                .ReturnsAsync(expectedSession);

            // Act
            var result = await _busTourService.CreateObiletSessionAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedSession.SessionId, result.SessionId);
            Assert.Equal(expectedSession.DeviceId, result.DeviceId);
            _mockExternalApi.Verify(x => x.GetSessionAsync(), Times.Once);
        }

        [Fact]
        public async Task GetBusLocationsAsync_WithValidParameters_ReturnsLocations()
        {
            // Arrange
            var sessionId = "test-session-id";
            var deviceId = "test-device-id";
            var expectedLocations = TestDataBuilder.CreateMockLocations();
            _mockExternalApi.Setup(x => x.GetBusLocationsAsync(sessionId, deviceId, null)).ReturnsAsync(expectedLocations);

            // Act
            var result = await _busTourService.GetBusLocationsAsync(sessionId, deviceId, null);

            // Assert
            Assert.Equal(expectedLocations.Count, result.Count());
        }

        [Fact]
        public async Task GetBusLocationsAsync_WithSearchTerm_ReturnsFilteredLocations()
        {
            // Arrange
            var session = TestDataBuilder.CreateMockSession();
            var expectedLocations = TestDataBuilder.CreateMockLocations();
            var searchTerm = "Istanbul";
            
            _mockExternalApi.Setup(x => x.GetSessionAsync())
                .ReturnsAsync(session);
            _mockExternalApi.Setup(x => x.GetBusLocationsAsync(session.SessionId, session.DeviceId, searchTerm))
                .ReturnsAsync(expectedLocations);

            // Act
            var result = await _busTourService.GetBusLocationsAsync(session.SessionId, session.DeviceId, searchTerm);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedLocations.Count, result.Count());
        }
    }
} 