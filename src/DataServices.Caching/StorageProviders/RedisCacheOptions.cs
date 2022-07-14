namespace DataServices.Caching.StorageProviders
{
    /// <summary>
    /// Options for redis cache backing store.
    /// </summary>
    public class RedisCacheOptions
    {
        /// <summary>
        /// Redis connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Optional instance name
        /// </summary>
        public string? InstanceName { get; set; }
    }
}
