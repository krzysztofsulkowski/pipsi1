using Microsoft.Extensions.Caching.Memory;

namespace TimeCapsule.Extentions
{
    public static class CacheExtensions
    {
        public static async Task<T> GetOrSetAsync<T>(this IMemoryCache cache, string cacheKey, TimeSpan expiration, Func<Task<T>> loadData)
        {
            if (!cache.TryGetValue(cacheKey, out T cachedData))
            {
                cachedData = await loadData();
                cache.Set(cacheKey, cachedData, expiration);
            }
            return cachedData;
        }

        public static void RemoveFromCache(this IMemoryCache cache, string key)
        {
            cache.Remove(key);
        }
    }
}

