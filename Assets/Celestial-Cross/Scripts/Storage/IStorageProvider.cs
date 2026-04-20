using System.Threading.Tasks;

namespace CelestialCross.Storage
{
    public interface IStorageProvider
    {
        Task SaveAsync(string key, string data);
        Task<string> LoadAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task DeleteAsync(string key);
    }
}
