using Infrastructure.Services;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Web.Controllers;
using Xunit;

namespace Test.Controllers
{
    public class CacheControllerTests
    {
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<CacheInvalidationService>> _mockCacheInvalidationLogger;
        private readonly Mock<ILogger<CacheController>> _mockLogger;
        private readonly CacheInvalidationService _cacheInvalidationService;
        private readonly CacheController _controller;

        public CacheControllerTests()
        {
            _mockCacheService = new Mock<ICacheService>();
            _mockCacheInvalidationLogger = new Mock<ILogger<CacheInvalidationService>>();
            _mockLogger = new Mock<ILogger<CacheController>>();
            _cacheInvalidationService = new CacheInvalidationService(_mockCacheService.Object, _mockCacheInvalidationLogger.Object);
            _controller = new CacheController(_cacheInvalidationService, _mockLogger.Object);
        }

        [Fact]
        public void InvalidateSessionCache_WithValidSessionId_ReturnsOkResult()
        {
            // Arrange
            var sessionId = "test-session-id";
            _mockCacheService.Setup(x => x.Remove(It.IsAny<string>())).Verifiable();

            // Act
            var result = _controller.InvalidateSessionCache(sessionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(new { message = $"Oturum önbelleği geçersiz kılındı: {sessionId}" }.ToString(), okResult.Value.ToString());
        }

        [Fact]
        public void InvalidateLocationCache_WithSearchTerm_ReturnsOkResult()
        {
            // Arrange
            var searchTerm = "Istanbul";
            _mockCacheService.Setup(x => x.Remove(It.IsAny<string>())).Verifiable();

            // Act
            var result = _controller.InvalidateLocationCache(searchTerm);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(new { message = $"Lokasyon önbelleği arama terimi için geçersiz kılındı: {searchTerm}" }.ToString(), okResult.Value.ToString());
        }

        [Fact]
        public void InvalidateJourneyCache_WithValidParameters_ReturnsOkResult()
        {
            // Arrange
            var originId = "1";
            var destinationId = "2";
            var departureDate = new DateTime(2024, 1, 15);
            _mockCacheService.Setup(x => x.Remove(It.IsAny<string>())).Verifiable();

            // Act
            var result = _controller.InvalidateJourneyCache(originId, destinationId, departureDate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(new { message = $"Sefer önbelleği geçersiz kılındı: {originId} noktasından {destinationId} noktasına {departureDate:yyyy-MM-dd} tarihinde" }.ToString(), okResult.Value.ToString());
        }

        [Fact]
        public void ClearAllCache_ReturnsOkResult()
        {
            // Arrange
            _mockCacheService.Setup(x => x.Clear()).Verifiable();

            // Act
            var result = _controller.ClearAllCache();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(new { message = "Tüm önbellek temizlendi" }.ToString(), okResult.Value.ToString());
        }

        [Fact]
        public void GetCacheStatus_ReturnsOkResultWithStatus()
        {
            // Act
            var result = _controller.GetCacheStatus();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public void InvalidateSessionCache_WhenServiceThrowsException_ReturnsOkResult()
        {
            // Arrange
            var sessionId = "test-session-id";
            _mockCacheService.Setup(x => x.Remove(It.IsAny<string>())).Throws(new Exception("Test exception"));

            // Act
            var result = _controller.InvalidateSessionCache(sessionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(new { message = $"Oturum önbelleği geçersiz kılındı: {sessionId}" }.ToString(), okResult.Value.ToString());
        }
    }
} 