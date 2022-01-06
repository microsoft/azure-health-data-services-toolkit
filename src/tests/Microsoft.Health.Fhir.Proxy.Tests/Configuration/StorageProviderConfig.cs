using Newtonsoft.Json;
using System;

namespace Microsoft.Health.Fhir.Proxy.Tests.Configuration
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



