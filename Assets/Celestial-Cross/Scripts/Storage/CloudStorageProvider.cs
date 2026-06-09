using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;

namespace CelestialCross.Storage
{
    public class CloudStorageProvider : IStorageProvider
    {
        public async Task SaveAsync(string key, string data)
        {
            var dict = new Dictionary<string, object> { { key, data } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(dict);
        }

        public async Task<string> LoadAsync(string key)
        {
            var results = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });
            if (results.TryGetValue(key, out var item))
            {
                return item.Value.GetAsString();
            }
            return null;
        }

        public async Task<bool> ExistsAsync(string key)
        {
            var results = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });
            return results.ContainsKey(key);
        }

        public async Task DeleteAsync(string key)
        {
            // Cloud Save não tem um "Delete" direto via chave simples da mesma forma que local, 
            // mas podemos limpar o valor ou usar métodos específicos se necessário.
            var dict = new Dictionary<string, object> { { key, null } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(dict);
        }
    }
}
