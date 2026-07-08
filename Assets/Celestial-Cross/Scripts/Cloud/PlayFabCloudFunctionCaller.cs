using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using PlayFab;
using PlayFab.CloudScriptModels;

namespace CelestialCross.Cloud
{
    public static class PlayFabCloudFunctionCaller
    {
        public static async Task<T> ExecuteFunctionAsync<T>(string functionName, object functionArgs)
        {
            if (!PlayFabClientAPI.IsClientLoggedIn())
            {
                Debug.LogWarning($"[Cloud] Tentativa de chamar {functionName} mas o PlayFab não está logado!");
                return default(T);
            }

            var tcs = new TaskCompletionSource<T>();

            var request = new ExecuteFunctionRequest
            {
                FunctionName = functionName,
                FunctionParameter = functionArgs,
                GeneratePlayStreamEvent = true
            };

            PlayFabCloudScriptAPI.ExecuteFunction(request, (ExecuteFunctionResult result) =>
            {
                if (result.FunctionResultTooLarge.HasValue && result.FunctionResultTooLarge.Value)
                {
                    Debug.LogError($"[Cloud] Erro: O resultado da função {functionName} foi muito grande.");
                    tcs.SetResult(default(T));
                    return;
                }

                if (result.Error != null)
                {
                    Debug.LogError($"[Cloud] Erro na função {functionName}: {result.Error.Message} | {result.Error.StackTrace}");
                    tcs.SetResult(default(T));
                    return;
                }

                if (result.FunctionResult != null)
                {
                    try
                    {
                        string jsonResult = result.FunctionResult.ToString();
                        T deserialized = JsonUtility.FromJson<T>(jsonResult);
                        tcs.SetResult(deserialized);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[Cloud] Erro ao deserializar o resultado de {functionName}: {e.Message}");
                        tcs.SetResult(default(T));
                    }
                }
                else
                {
                    tcs.SetResult(default(T));
                }

            }, (PlayFabError error) =>
            {
                Debug.LogError($"[Cloud] Erro de rede ao chamar {functionName}: {error.GenerateErrorReport()}");
                tcs.SetResult(default(T));
            });

            return await tcs.Task;
        }
    }
}
