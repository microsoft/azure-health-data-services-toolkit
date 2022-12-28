using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.AzureHealth.DataServices.Security
{
    /// <summary>
    /// A Token cache class is used to store the token in In-Memory caching.
    /// </summary>
    public class TokenCache : ITokenCache
    {
        /// <summary>
        /// Create an Instance of Token Cache.
        /// </summary>
        /// <param name="memoryCache"></param>
        /// <param name="logger"></param>
        public TokenCache(IMemoryCache memoryCache, ILogger<TokenCache> logger = null)
        {
            this._memoryCache = memoryCache;
            this.logger = logger;
            this.keyLocker = new ConcurrentDictionary<string, SemaphoreSlim>();
        }

        private readonly IMemoryCache _memoryCache;
        private readonly ILogger logger;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> keyLocker;
        private int expiryTimeOut = 120000;

        /// <summary>
        /// Add the Token to the In Memory Cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task AddToken(string key, string value)
        {
            var keyLock = keyLocker.GetOrAdd(key, x => new SemaphoreSlim(1));
            await keyLock.WaitAsync();

            try
            {
                string json = JsonConvert.SerializeObject(value);
                _memoryCache.Set(key, json, GetOptions());
                logger?.LogTrace("Key {key} set to local memory cache.", key);
            }
            finally
            {
                keyLock?.Release();
            }
        }
        /// <summary>
        /// Get the Token from the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<string> GetToken(string key)
        {
            var keyLock = keyLocker.GetOrAdd(key, x => new SemaphoreSlim(1));
            await keyLock.WaitAsync();

            try
            {
                if (_memoryCache.TryGetValue(key, out string value))
                {
                    return JsonConvert.DeserializeObject<string>(value);
                }

                logger?.LogTrace("Key {key} not found in local memory cache.", key);
                return string.Empty;
            }
            finally
            {
                keyLock?.Release();
            }
        }

        private MemoryCacheEntryOptions GetOptions()
        {
            MemoryCacheEntryOptions options = new()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(expiryTimeOut)
            };

            _ = options.RegisterPostEvictionCallback(OnPostEviction);

            return options;
        }

        private void OnPostEviction(object key, object letter, EvictionReason reason, object state)
        {
            logger?.LogTrace("Key {key} evicted from cache.", key);
        }


    }
}
