using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

namespace CelestialCross.Storage
{
    public class CloudStorageProvider : IStorageProvider
    {
        public async Task SaveAsync(string key, string data)
        {
            if (!PlayFabClientAPI.IsClientLoggedIn()) return;

            var tcs = new TaskCompletionSource<bool>();
            PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest {
                Data = new Dictionary<string, string> { { key, data } }
            }, res => tcs.SetResult(true), err => {
                Debug.LogError($"[CloudStorage] Erro ao salvar {key}: {err.ErrorMessage}");
                tcs.SetResult(false);
            });
            await tcs.Task;
        }

        public async Task<string> LoadAsync(string key)
        {
            if (!PlayFabClientAPI.IsClientLoggedIn()) return null;

            var tcs = new TaskCompletionSource<string>();
            PlayFabClientAPI.GetUserData(new GetUserDataRequest {
                Keys = new List<string> { key }
            }, res => {
                if (res.Data != null && res.Data.ContainsKey(key))
                    tcs.SetResult(res.Data[key].Value);
                else
                    tcs.SetResult(null);
            }, err => {
                Debug.LogError($"[CloudStorage] Erro ao carregar {key}: {err.ErrorMessage}");
                tcs.SetResult(null);
            });
            return await tcs.Task;
        }

        public async Task<bool> ExistsAsync(string key)
        {
            string data = await LoadAsync(key);
            return !string.IsNullOrEmpty(data);
        }

        public async Task DeleteAsync(string key)
        {
            await SaveAsync(key, null); // Deleta atribuindo null
        }
    }
}
