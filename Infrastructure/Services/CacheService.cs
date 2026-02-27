using Application.Abstractions;
using Microsoft.Extensions.Caching.Memory;


namespace Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public bool Exists(string key) => _memoryCache.TryGetValue(key, out _);

        public T Get<T>(string key) => _memoryCache.Get<T>(key);

        public void Set<T>(string key, T value) => _memoryCache.Set(key, value);

        public void Remove(string key) => _memoryCache.Remove(key);
    }

}
