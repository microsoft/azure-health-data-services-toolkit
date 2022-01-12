namespace Microsoft.Health.Fhir.Proxy.Caching.StorageProviders
{
    public class RedisCacheOptions
    {
        public string ConnectionString { get; set; }

        public string? InstanceName { get; set; }
    }
}
