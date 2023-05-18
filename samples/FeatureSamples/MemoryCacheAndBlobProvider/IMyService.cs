using System.Threading.Tasks;

namespace MemoryCacheAndBlobProvider
{
    public interface IMyService
    {
        Task SetAsync<T>(string key, T item);

        Task<T> GetAsync<T>(string key)
            where T : class;
    }
}
