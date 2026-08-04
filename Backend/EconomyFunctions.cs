using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ServerModels;
using System.Collections.Generic;

namespace CelestialCross.Backend
{
    public class EconomyFunctions
    {
        private readonly ILogger _logger;
        private readonly PlayFabServerInstanceAPI _serverApi;

        public EconomyFunctions(ILoggerFactory loggerFactory, PlayFabServerInstanceAPI serverApi)
        {
            _logger = loggerFactory.CreateLogger<EconomyFunctions>();
            _serverApi = serverApi;
        }

        public class EconomyData
        {
            public int Money { get; set; }
            public int StarMaps { get; set; }
            public int Stardust { get; set; }
        }

        private async Task<EconomyData> GetEconomyDataAsync(string playFabId)
        {
            var dataResult = await _serverApi.GetUserInternalDataAsync(new GetUserDataRequest
            {
                PlayFabId = playFabId,
                Keys = new List<string> { "Economy" }
            });

            if (dataResult.Result.Data != null && dataResult.Result.Data.ContainsKey("Economy"))
            {
                return JsonConvert.DeserializeObject<EconomyData>(dataResult.Result.Data["Economy"].Value);
            }
            
            return new EconomyData { Money = 100, StarMaps = 0, Stardust = 0 };
        }

        private async Task SaveEconomyDataAsync(string playFabId, EconomyData data)
        {
            await _serverApi.UpdateUserInternalDataAsync(new UpdateUserInternalDataRequest
            {
                PlayFabId = playFabId,
                Data = new Dictionary<string, string> { { "Economy", JsonConvert.SerializeObject(data) } }
            });
        }

        [Function("GetPlayerEconomy")]
        public async Task<HttpResponseData> GetPlayerEconomy(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            var (playFabId, context, _) = await AuthHelper.GetContextAsync(req);
            
            if (string.IsNullOrEmpty(playFabId))
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { Success = false, ErrorMessage = "Jogador não autenticado." });
                return badResp;
            }

            var ecoData = await GetEconomyDataAsync(playFabId);
            
            var okResp = req.CreateResponse(HttpStatusCode.OK);
            await okResp.WriteAsJsonAsync(new { Success = true, Economy = ecoData });
            return okResp;
        }

        [Function("SpendCurrency")]
        public async Task<HttpResponseData> SpendCurrency(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            var (playFabId, context, _) = await AuthHelper.GetContextAsync(req);
            
            if (string.IsNullOrEmpty(playFabId))
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { Success = false, ErrorMessage = "Jogador não autenticado." });
                return badResp;
            }

            var args = context?.FunctionArgument;
            string currencyType = args?.CurrencyType;
            int amount = (int)(args?.Amount ?? 0);

            if (string.IsNullOrEmpty(currencyType) || amount <= 0)
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { Success = false, ErrorMessage = "Argumentos inválidos." });
                return badResp;
            }

            var ecoData = await GetEconomyDataAsync(playFabId);

            bool success = false;
            if (currencyType == "Money" && ecoData.Money >= amount)
            {
                ecoData.Money -= amount;
                success = true;
            }
            else if (currencyType == "StarMaps" && ecoData.StarMaps >= amount)
            {
                ecoData.StarMaps -= amount;
                success = true;
            }
            else if (currencyType == "Stardust" && ecoData.Stardust >= amount)
            {
                ecoData.Stardust -= amount;
                success = true;
            }

            if (!success)
            {
                var okFailResp = req.CreateResponse(HttpStatusCode.OK);
                await okFailResp.WriteAsJsonAsync(new { Success = false, ErrorMessage = $"Saldo insuficiente de {currencyType}." });
                return okFailResp;
            }

            await SaveEconomyDataAsync(playFabId, ecoData);

            var okResp = req.CreateResponse(HttpStatusCode.OK);
            await okResp.WriteAsJsonAsync(new { Success = true, Economy = ecoData });
            return okResp;
        }

        [Function("AddCurrency")]
        public async Task<HttpResponseData> AddCurrency(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            var (playFabId, context, _) = await AuthHelper.GetContextAsync(req);
            
            if (string.IsNullOrEmpty(playFabId))
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { Success = false, ErrorMessage = "Jogador não autenticado." });
                return badResp;
            }

            // ATENÇÃO: Esta função deve ser protegida/auditada, pois é usada para recompensas
            // Por enquanto vamos aceitar a chamada, mas no futuro deve validar se veio de um fluxo válido (ex: reward de batalha)

            var args = context?.FunctionArgument;
            string currencyType = args?.CurrencyType;
            int amount = (int)(args?.Amount ?? 0);

            if (string.IsNullOrEmpty(currencyType) || amount <= 0)
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { Success = false, ErrorMessage = "Argumentos inválidos." });
                return badResp;
            }

            var ecoData = await GetEconomyDataAsync(playFabId);

            if (currencyType == "Money") ecoData.Money += amount;
            else if (currencyType == "StarMaps") ecoData.StarMaps += amount;
            else if (currencyType == "Stardust") ecoData.Stardust += amount;
            else 
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { Success = false, ErrorMessage = "Moeda desconhecida." });
                return badResp;
            }

            await SaveEconomyDataAsync(playFabId, ecoData);

            var okResp = req.CreateResponse(HttpStatusCode.OK);
            await okResp.WriteAsJsonAsync(new { Success = true, Economy = ecoData });
            return okResp;
        }
    }
}
