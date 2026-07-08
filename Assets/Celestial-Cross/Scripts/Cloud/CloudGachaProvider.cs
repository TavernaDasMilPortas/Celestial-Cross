using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CelestialCross.Data;
using CelestialCross.Gacha;

namespace CelestialCross.Cloud
{
    public class CloudGachaProvider : IGachaProvider
    {
        [global::System.Serializable]
        private class GachaRequestData
        {
            public string BannerID;
            public int PullQuantity;
        }

        public async Task<List<RuntimeGachaResult>> PullAsync(Account account, GachaBannerSO banner, int times)
        {
            var requestData = new GachaRequestData
            {
                BannerID = banner.BannerID,
                PullQuantity = times
            };

            Debug.Log($"[CloudGachaProvider] Solicitando {times} pulls no banner {banner.BannerID} via Azure Functions...");

            // Chama a Azure Function usando object temporariamente para não falhar na desserialização 
            // até que a function na nuvem devolva os dados compatíveis com RuntimeGachaResult.
            var rawResponse = await PlayFabCloudFunctionCaller.ExecuteFunctionAsync<object>("ExecuteGacha", requestData);

            if (rawResponse == null)
            {
                Debug.LogWarning("[CloudGachaProvider] A resposta da nuvem foi nula (PlayFab nÃ£o integrado totalmente). Caindo para geraÃ§Ã£o local por enquanto (Fallback).");
                
                // FALLBACK TEMPORÃ RIO PARA DESENVOLVIMENTO
                // Usa a lÃ³gica local se a nuvem falhar enquanto o PlayFab nÃ£o estÃ¡ 100% integrado
                return await GachaService.Instance.ExecutePullsInternalAsync(account, banner, times);
            }

            // Como a função da nuvem ainda é um "Stub" e retorna apenas IDs de string,
            // forçamos o fallback local para que as unidades/pets/artefatos sejam de fato instanciados na conta.
            return await GachaService.Instance.ExecutePullsInternalAsync(account, banner, times);
        }
    }
}
