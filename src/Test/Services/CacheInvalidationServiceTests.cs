using Application.Interfaces;
using Domain.Constants;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Test.Helpers;
using Xunit;

namespace Test.Services
{
    public class CacheInvalidationServiceTests
    {
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<CacheInvalidationService>> _mockLogger;
        private readonly CacheInvalidationService _cacheInvalidationService;

        public CacheInvalidationServiceTests()
        {
            _mockCacheService = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<CacheInvalidationService>>();
            _cacheInvalidationService = new CacheInvalidationService(_mockCacheService.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_WithNullCacheService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new CacheInvalidationService(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new CacheInvalidationService(_mockCacheService.Object, null!));
        }

        [Fact]
        public void InvalidateSessionCache_WithValidSessionId_CallsRemoveWithCorrectKey()
        {
            // Arrange
            var sessionId = "test-session-id";
            var expectedKey = CacheKeys.SessionKey(sessionId);

            // Act
            _cacheInvalidationService.InvalidateSessionCache(sessionId);

            // Assert
            _mockCacheService.Verify(x => x.Remove(expectedKey), Times.Once);
        }

        [Fact]
        public void InvalidateLocationCache_WithSearchTerm_CallsRemoveWithCorrectKey()
        {
            // Arrange
            var searchTerm = "Istanbul";
            var expectedKey = CacheKeys.LocationsSearchKey(searchTerm);

            // Act
            _cacheInvalidationService.InvalidateLocationCache(searchTerm);

            // Assert
            _mockCacheService.Verify(x => x.Remove(expectedKey), Times.Once);
        }

        [Fact]
        public void InvalidateJourneyCache_WithValidParameters_CallsRemoveWithCorrectKey()
        {
            // Arrange
            var originId = "1";
            var destinationId = "2";
            var departureDate = new DateTime(2024, 1, 15);
            var expectedKey = CacheKeys.JourneysKey(originId, destinationId, departureDate);

            // Act
            _cacheInvalidationService.InvalidateJourneyCache(originId, destinationId, departureDate);

            // Assert
            _mockCacheService.Verify(x => x.Remove(expectedKey), Times.Once);
        }

        [Fact]
        public void ClearAllCache_CallsClearOnCacheService()
        {
            // Act
            _cacheInvalidationService.ClearAllCache();

            // Assert
            _mockCacheService.Verify(x => x.Clear(), Times.Once);
        }

        [Fact]
        public void InvalidateSessionCache_WhenCacheServiceThrowsException_LogsError()
        {
            // Arrange
            var sessionId = "test-session-id";
            var exception = new Exception("Cache error");
            _mockCacheService.Setup(x => x.Remove(It.IsAny<string>()))
                .Throws(exception);

            // Act
            _cacheInvalidationService.InvalidateSessionCache(sessionId);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
} 