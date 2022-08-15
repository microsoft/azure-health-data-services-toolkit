using System;
using Newtonsoft.Json;

namespace Azure.Health.DataServices.Tests.Configuration
{

    [Serializable]
    [JsonObject]
    public class StorageProviderConfig
    {
        public StorageProviderConfig()
        {

        }

        public StorageProviderConfig(double expiry, string connectionString)
        {
            CacheExpirationMilliseconds = expiry;
            CacheConnectionString = connectionString;
        }

        [JsonProperty("cacheConnectionString")]
        public string CacheConnectionString { get; set; }

        [JsonProperty("cacheExpirationMilliseconds")]
        public double CacheExpirationMilliseconds { get; set; }
    }
}



