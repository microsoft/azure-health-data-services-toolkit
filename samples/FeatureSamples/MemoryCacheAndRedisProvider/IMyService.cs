using System.Threading.Tasks;

namespace MemoryCacheAndRedisProvider
{
    public interface IMyService
    {
        Task SetAsync<T>(string key, T item);
        Task<T> GetAsync<T>(string key) where T : class;
    }
}
