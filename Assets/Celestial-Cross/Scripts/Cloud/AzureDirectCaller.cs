using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using PlayFab;
using Newtonsoft.Json;

namespace CelestialCross.Cloud
{
    public static class AzureDirectCaller
    {
        private static AzureConfig _config;

        public static AzureConfig Config
        {
            get
            {
                if (_config == null)
                {
                    _config = Resources.Load<AzureConfig>("AzureConfig");
                    if (_config == null)
                    {
                        Debug.LogWarning("[AzureDirectCaller] Arquivo AzureConfig não encontrado em Resources! As chamadas diretas falharão.");
                    }
                }
                return _config;
            }
        }

        public static async Task<T> CallAsync<T>(string functionName, object payload = null)
        {
            if (Config == null || string.IsNullOrEmpty(Config.BaseUrl))
            {
                Debug.LogError("[AzureDirectCaller] AzureConfig não configurado. Impossível chamar Azure.");
                return default(T);
            }

            if (!PlayFabClientAPI.IsClientLoggedIn())
            {
                Debug.LogWarning($"[AzureDirectCaller] Tentativa de chamar {functionName} mas o PlayFab não está logado!");
                return default(T);
            }

            string playFabId = CelestialCross.Authentication.PlayFabAuthManager.Instance?.PlayFabId;
            if (string.IsNullOrEmpty(playFabId))
            {
                Debug.LogWarning($"[AzureDirectCaller] PlayFabId não encontrado!");
                return default(T);
            }

            string url = $"{Config.BaseUrl.Trim().TrimEnd('/')}/{functionName}";
            
            // Cria um payload embrulhado, compatível com a expectativa das Functions que migraram do CloudScript
            var wrappedPayload = new 
            {
                PlayFabId = playFabId,
                FunctionArgument = payload
            };

            string jsonPayload = JsonConvert.SerializeObject(wrappedPayload);
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("x-playfab-id", playFabId); // Header opcional de segurança
                
                if (!string.IsNullOrEmpty(Config.FunctionKey))
                {
                    request.SetRequestHeader("x-functions-key", Config.FunctionKey);
                }

                request.timeout = Config.TimeoutSeconds;

                // SendWebRequest is not directly awaitable in older Unity, need a custom extension or TaskCompletionSource.
                // Since this project might not have UniTask everywhere, let's use a TaskCompletionSource manually.
                var tcs = new TaskCompletionSource<T>();
                
                var operation = request.SendWebRequest();
                operation.completed += (asyncOp) => 
                {
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"[AzureDirectCaller] Erro HTTP chamando {functionName}: {request.error} | Resposta: {request.downloadHandler?.text}");
                        tcs.TrySetResult(default(T));
                        return;
                    }

                    try
                    {
                        string resultText = request.downloadHandler.text;
                        T deserialized = JsonConvert.DeserializeObject<T>(resultText);
                        tcs.TrySetResult(deserialized);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[AzureDirectCaller] Erro ao deserializar o resultado de {functionName}: {ex.Message}");
                        tcs.TrySetResult(default(T));
                    }
                };

                return await tcs.Task;
            }
        }
    }
}
