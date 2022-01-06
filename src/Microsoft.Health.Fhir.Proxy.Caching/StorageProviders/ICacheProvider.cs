namespace Microsoft.Health.Fhir.Proxy.Caching.StorageProviders
{
    public interface ICacheProvider
    {
        Task AddAsync<T>(string key, T value);

        Task<T> GetAsync<T>(string key);

        Task<bool> RemoveAsync(string key);

    }
}
