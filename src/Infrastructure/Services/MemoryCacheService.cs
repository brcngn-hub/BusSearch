using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;

        public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public T? Get<T>(string key)
        {
            try
            {
                return _memoryCache.Get<T>(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Önbellek öğesi alınırken hata oluştu. Anahtar: {Key}", key);
                return default;
            }
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                return await Task.FromResult(_memoryCache.Get<T>(key));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Önbellek öğesi alınırken hata oluştu. Anahtar: {Key}", key);
                return default;
            }
        }

        public void Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var options = new MemoryCacheEntryOptions();
                
                if (expiration.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiration;
                }
                
                _memoryCache.Set(key, value, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Önbellek öğesi ayarlanırken hata oluştu. Anahtar: {Key}", key);
            }
        }

        public void Remove(string key)
        {
            try
            {
                _memoryCache.Remove(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Önbellek öğesi kaldırılırken hata oluştu. Anahtar: {Key}", key);
            }
        }

        public bool TryGet<T>(string key, out T? value)
        {
            try
            {
                return _memoryCache.TryGetValue(key, out value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Önbellek öğesi alınmaya çalışılırken hata oluştu. Anahtar: {Key}", key);
                value = default;
                return false;
            }
        }

        public void Clear()
        {
            try
            {
                if (_memoryCache is MemoryCache memoryCache)
                {
                    memoryCache.Compact(1.0);
                }
                _logger.LogInformation("Tüm önbellek öğeleri temizlendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Önbellek temizlenirken hata oluştu");
            }
        }
    }
} 