using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Web.Middleware;
using Xunit;

namespace Test.Middleware
{
    public class RateLimitMiddlewareTests
    {
        private readonly Mock<ILogger<RateLimitMiddleware>> _mockLogger;
        private readonly RateLimitMiddleware _middleware;

        public RateLimitMiddlewareTests()
        {
            _mockLogger = new Mock<ILogger<RateLimitMiddleware>>();
            _middleware = new RateLimitMiddleware(
                next: (context) => Task.CompletedTask,
                logger: _mockLogger.Object
            );
        }

        [Fact]
        public void Constructor_WithNullNext_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new RateLimitMiddleware(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new RateLimitMiddleware((context) => Task.CompletedTask, null!));
        }

        [Fact]
        public async Task InvokeAsync_WithNormalRequest_DoesNotThrowException()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.StatusCode = 200;

            // Act & Assert
            await _middleware.InvokeAsync(context);
            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_WhenStatusCodeIs429_LogsAndSetsStatusCode()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.StatusCode = 429;

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(429, context.Response.StatusCode);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
} 