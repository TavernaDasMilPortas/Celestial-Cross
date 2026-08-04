using System;
using System.Threading.Tasks;

namespace CelestialCross.Cloud
{
    public static class PlayFabCloudFunctionCaller
    {
        /// <summary>
        /// Wrapper legado que agora redireciona todas as chamadas para a comunicação HTTP direta com o Azure Functions.
        /// Isso elimina o hop de rede extra causado pelo PlayFab CloudScript (redução de 200-500ms de latência por chamada).
        /// </summary>
        public static async Task<T> ExecuteFunctionAsync<T>(string functionName, object functionArgs)
        {
            return await AzureDirectCaller.CallAsync<T>(functionName, functionArgs);
        }
    }
}
