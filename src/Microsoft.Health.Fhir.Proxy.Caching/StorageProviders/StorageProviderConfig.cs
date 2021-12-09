using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Proxy.Caching.StorageProviders
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
