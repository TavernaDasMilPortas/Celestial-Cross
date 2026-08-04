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
    public class EnergyFunctions
    {
        private readonly ILogger _logger;
        private readonly PlayFabServerInstanceAPI _serverApi;

        public EnergyFunctions(ILoggerFactory loggerFactory, PlayFabServerInstanceAPI serverApi)
        {
            _logger = loggerFactory.CreateLogger<EnergyFunctions>();
            _serverApi = serverApi;
        }

        [Function("GetServerTime")]
        public async Task<HttpResponseData> GetServerTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            _logger.LogInformation("GetServerTime function processed a request.");
            
            var responseData = new { ServerTimeUTC = DateTime.UtcNow.ToString("O") };
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(responseData);
            
            return response;
        }

        public class ConsumeEnergyResponse
        {
            public bool Success { get; set; }
            public int CurrentEnergy { get; set; }
            public string LastRegenTimestampUTC { get; set; }
            public string ErrorMessage { get; set; }
        }

        [Function("ConsumeEnergy")]
        public async Task<HttpResponseData> ConsumeEnergy(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            _logger.LogInformation("ConsumeEnergy: Processando requisição...");

            var (playFabId, context, _) = await AuthHelper.GetContextAsync(req);
            
            if (string.IsNullOrEmpty(playFabId))
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new ConsumeEnergyResponse { Success = false, ErrorMessage = "Jogador não autenticado." });
                return badResp;
            }

            var args = context?.FunctionArgument;
            int amountToConsume = 0;
            if (args != null && args.Amount != null)
            {
                amountToConsume = (int)args.Amount;
            }

            if (amountToConsume <= 0)
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new ConsumeEnergyResponse { Success = false, ErrorMessage = "Quantidade inválida." });
                return badResp;
            }

            var getDataRequest = new GetUserDataRequest
            {
                PlayFabId = playFabId,
                Keys = new List<string> { "EnergyData" }
            };

            var dataResult = await _serverApi.GetUserDataAsync(getDataRequest);
            if (dataResult.Error != null)
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new ConsumeEnergyResponse { Success = false, ErrorMessage = "Erro ao buscar dados." });
                return badResp;
            }

            int currentEnergy = 100;
            string lastRegenTimestamp = DateTime.UtcNow.ToString("O");

            if (dataResult.Result.Data != null && dataResult.Result.Data.ContainsKey("EnergyData"))
            {
                dynamic energyData = JsonConvert.DeserializeObject(dataResult.Result.Data["EnergyData"].Value);
                currentEnergy = (int)energyData.CurrentEnergy;
                lastRegenTimestamp = (string)energyData.LastRegenTimestampUTC;
            }

            if (currentEnergy < amountToConsume)
            {
                var okFailResp = req.CreateResponse(HttpStatusCode.OK);
                await okFailResp.WriteAsJsonAsync(new ConsumeEnergyResponse { Success = false, ErrorMessage = "Energia Insuficiente na nuvem." });
                return okFailResp;
            }

            currentEnergy -= amountToConsume;
            int maxEnergy = 100;
            if (currentEnergy + amountToConsume >= maxEnergy && currentEnergy < maxEnergy)
            {
                lastRegenTimestamp = DateTime.UtcNow.ToString("O");
            }

            var updateDataRequest = new UpdateUserDataRequest
            {
                PlayFabId = playFabId,
                Data = new Dictionary<string, string>
                {
                    { 
                        "EnergyData", JsonConvert.SerializeObject(new { 
                            CurrentEnergy = currentEnergy, 
                            LastRegenTimestampUTC = lastRegenTimestamp 
                        }) 
                    }
                }
            };

            var updateResult = await _serverApi.UpdateUserDataAsync(updateDataRequest);
            if (updateResult.Error != null)
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new ConsumeEnergyResponse { Success = false, ErrorMessage = "Erro ao salvar energia descontada." });
                return badResp;
            }

            var okResp = req.CreateResponse(HttpStatusCode.OK);
            await okResp.WriteAsJsonAsync(new ConsumeEnergyResponse 
            { 
                Success = true, 
                CurrentEnergy = currentEnergy,
                LastRegenTimestampUTC = lastRegenTimestamp
            });
            return okResp;
        }
    }
}
