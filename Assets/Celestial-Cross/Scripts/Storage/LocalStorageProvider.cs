using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace CelestialCross.Storage
{
    public class LocalStorageProvider : IStorageProvider
    {
        private string GetPath(string key) => Path.Combine(Application.persistentDataPath, $"{key}.json");

        public Task SaveAsync(string key, string data)
        {
            File.WriteAllText(GetPath(key), data);
            return Task.CompletedTask;
        }

        public Task<string> LoadAsync(string key)
        {
            string path = GetPath(key);
            if (File.Exists(path))
            {
                return Task.FromResult(File.ReadAllText(path));
            }
            return Task.FromResult<string>(null);
        }

        public Task<bool> ExistsAsync(string key)
        {
            return Task.FromResult(File.Exists(GetPath(key)));
        }

        public Task DeleteAsync(string key)
        {
            string path = GetPath(key);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            return Task.CompletedTask;
        }
    }
}
