using Azure.Health.DataServices.Caching;
using System.Threading.Tasks;

namespace MemoryCacheAndRedisProvider
{
    public class MyService : IMyService
    {
        public MyService(IJsonObjectCache cache)
        {
            this.cache = cache;
        }

        private readonly IJsonObjectCache cache;

        public async Task<T> GetAsync<T>(string key) where T : class
        {
            return await cache.GetAsync<T>(key);
        }

        public async Task SetAsync<T>(string key, T item)
        {
            await cache.AddAsync(key, item);
        }
    }
}
