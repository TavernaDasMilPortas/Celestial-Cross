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

        public EnergyFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<EnergyFunctions>();
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

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic context = JsonConvert.DeserializeObject(requestBody);
            
            string playFabId = context?.CallerEntityProfile?.Lineage?.MasterPlayerAccountId;
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

            var apiSettings = new PlayFabApiSettings
            {
                TitleId = Environment.GetEnvironmentVariable("PLAYFAB_TITLE_ID") ?? "SEU_TITLE_ID",
                DeveloperSecretKey = Environment.GetEnvironmentVariable("PLAYFAB_DEV_SECRET_KEY") ?? "SUA_SECRET_KEY"
            };
            var serverApi = new PlayFabServerInstanceAPI(apiSettings);

            var getDataRequest = new GetUserDataRequest
            {
                PlayFabId = playFabId,
                Keys = new List<string> { "EnergyData" }
            };

            var dataResult = await serverApi.GetUserDataAsync(getDataRequest);
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

            var updateResult = await serverApi.UpdateUserDataAsync(updateDataRequest);
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
