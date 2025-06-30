using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Test.Helpers;
using Xunit;

namespace Test.Services
{
    public class MemoryCacheServiceTests
    {
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<ILogger<MemoryCacheService>> _mockLogger;
        private readonly ICacheService _cacheService;

        public MemoryCacheServiceTests()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _mockLogger = new Mock<ILogger<MemoryCacheService>>();
            _cacheService = new MemoryCacheService(_memoryCache, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_WithNullMemoryCache_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new MemoryCacheService(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new MemoryCacheService(_memoryCache, null!));
        }

        [Fact]
        public void Set_WithValidData_StoresDataInCache()
        {
            // Arrange
            var key = "test-key";
            var value = "test-value";

            // Act
            _cacheService.Set(key, value);

            // Assert
            var result = _cacheService.Get<string>(key);
            Assert.Equal(value, result);
        }

        [Fact]
        public void Get_WithExistingKey_ReturnsValue()
        {
            // Arrange
            var key = "test-key";
            var expectedValue = "test-value";
            _cacheService.Set(key, expectedValue);

            // Act
            var result = _cacheService.Get<string>(key);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void Get_WithNonExistingKey_ReturnsNull()
        {
            // Arrange
            var key = "non-existing-key";

            // Act
            var result = _cacheService.Get<string>(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Remove_WithExistingKey_RemovesFromCache()
        {
            // Arrange
            var key = "test-key";
            var value = "test-value";
            _cacheService.Set(key, value);

            // Act
            _cacheService.Remove(key);

            // Assert
            var result = _cacheService.Get<string>(key);
            Assert.Null(result);
        }

        [Fact]
        public void Clear_RemovesAllItemsFromCache()
        {
            // Arrange
            _cacheService.Set("key1", "value1");
            _cacheService.Set("key2", "value2");
            _cacheService.Set("key3", "value3");

            // Act
            _cacheService.Clear();

            // Assert
            Assert.Null(_cacheService.Get<string>("key1"));
            Assert.Null(_cacheService.Get<string>("key2"));
            Assert.Null(_cacheService.Get<string>("key3"));
        }
    }
} 