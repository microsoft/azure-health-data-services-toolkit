namespace Microsoft.Health.Fhir.Proxy.Caching
{
    /// <summary>
    /// Json cache options for IMemoryCache.
    /// </summary>
    public class JsonCacheOptions
    {
        /// <summary>
        /// Gets or sets the expiration time of a cached item.
        /// </summary>
        public TimeSpan CacheItemExpiry { get; set; }
    }
}
