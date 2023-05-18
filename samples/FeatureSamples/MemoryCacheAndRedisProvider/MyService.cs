using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Caching;

namespace MemoryCacheAndRedisProvider
{
    public class MyService : IMyService
    {
        private readonly IJsonObjectCache cache;

        public MyService(IJsonObjectCache cache)
        {
            this.cache = cache;
        }

        public async Task<T> GetAsync<T>(string key)
            where T : class
        {
            return await cache.GetAsync<T>(key);
        }

        public async Task SetAsync<T>(string key, T item)
        {
            await cache.AddAsync(key, item);
        }
    }
}
