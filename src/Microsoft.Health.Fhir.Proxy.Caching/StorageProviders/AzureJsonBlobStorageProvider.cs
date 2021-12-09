using Microsoft.Health.Fhir.Proxy.Storage;
using Newtonsoft.Json;
using System.Text;

namespace Microsoft.Health.Fhir.Proxy.Caching.StorageProviders
{
    public class AzureJsonBlobStorageProvider : IStorageProvider
    {
        private readonly StorageBlob storage;
        private readonly string container;

        public AzureJsonBlobStorageProvider(string connectionString, string container)
        {
            storage = new(connectionString);
            this.container = container;
        }

        public async Task AddAsync<T>(string key, T value)
        {
            string json = JsonConvert.SerializeObject(value);
            await storage.WriteBlockBlobAsync(container, $"{key}.json", "application/json", Encoding.UTF8.GetBytes(json));
        }

        public async Task<T> GetAsync<T>(string key)
        {
            byte[] content = await storage.ReadBlockBlobAsync(container, $"{key}.json");
            string contentString = Encoding.UTF8.GetString(content);
            return JsonConvert.DeserializeObject<T>(contentString);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await storage.DeleteBlobAsync(container, $"{key}.json");
        }
    }
}
