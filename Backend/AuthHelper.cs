using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System.Linq;

namespace CelestialCross.Backend
{
    public static class AuthHelper
    {
        public static async Task<string> ExtractPlayFabIdAsync(HttpRequestData req, dynamic context)
        {
            // 1. Tenta extrair do formato CloudScript antigo
            string playFabId = context?.CallerEntityProfile?.Lineage?.MasterPlayerAccountId;
            if (!string.IsNullOrEmpty(playFabId))
            {
                return playFabId;
            }

            // 2. Tenta extrair do Header x-playfab-id
            if (req.Headers.TryGetValues("x-playfab-id", out var headers))
            {
                string headerId = headers.FirstOrDefault();
                if (!string.IsNullOrEmpty(headerId))
                {
                    return headerId;
                }
            }

            // 3. Tenta extrair do payload direto
            if (context != null && context.PlayFabId != null)
            {
                return context.PlayFabId;
            }

            return null;
        }

        public static async Task<(string playFabId, dynamic context, string requestBody)> GetContextAsync(HttpRequestData req)
        {
            if (req.Body.CanSeek) req.Body.Position = 0; 
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic context = string.IsNullOrWhiteSpace(requestBody) ? null : JsonConvert.DeserializeObject(requestBody);
            string playFabId = await ExtractPlayFabIdAsync(req, context);
            return (playFabId, context, requestBody);
        }
    }
}
