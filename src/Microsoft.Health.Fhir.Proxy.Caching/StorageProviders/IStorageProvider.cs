﻿namespace Microsoft.Health.Fhir.Proxy.Caching.StorageProviders
{
    public interface IStorageProvider
    {
        Task AddAsync<T>(string key, T value);

        Task<T> GetAsync<T>(string key);

        Task<bool> RemoveAsync(string key);

    }
}
