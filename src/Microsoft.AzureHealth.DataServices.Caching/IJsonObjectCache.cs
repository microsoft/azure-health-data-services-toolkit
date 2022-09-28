namespace Microsoft.AzureHealth.DataServices.Caching
{
    /// <summary>namespace Microsoft.AzureHealth.DataServices.Caching
    /// Interface for JSON enabled cache.
    /// </summary>
    public interface IJsonObjectCache
    {
        /// <summary>
        /// Adds an object to the cache.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <param name="value">Item to cache.</param>
        /// <returns>Task.</returns>
        Task AddAsync<T>(string key, T value);

        /// <summary>
        /// Adds an object to the cache.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <param name="value">Item to cache.</param>
        /// <returns>Task.</returns>
        Task AddAsync(string key, object value);

        /// <summary>
        /// Gets an item from the cache.
        /// </summary>
        /// <typeparam name="T">Type of item to return from cache.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// Gets an object from the cache.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <returns>Cached object as a json string.</returns>
        Task<string> GetAsync(string key);

        /// <summary>
        /// Removes an item from the cache.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <returns>True if removed otherwise false.</returns>
        Task<bool> RemoveAsync(string key);

    }
}
