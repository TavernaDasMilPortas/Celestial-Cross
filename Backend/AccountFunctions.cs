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
    public class AccountFunctions
    {
        private readonly ILogger _logger;
        private readonly PlayFabServerInstanceAPI _serverApi;
        private static readonly Random rng = new Random();

        public AccountFunctions(ILoggerFactory loggerFactory, PlayFabServerInstanceAPI serverApi)
        {
            _logger = loggerFactory.CreateLogger<AccountFunctions>();
            _serverApi = serverApi;
        }

        [Function("InitializeAccount")]
        public async Task<HttpResponseData> InitializeAccount(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            _logger.LogInformation("InitializeAccount function triggered.");

            var (playFabId, context, _) = await AuthHelper.GetContextAsync(req);
            
            if (string.IsNullOrEmpty(playFabId))
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { Success = false, ErrorMessage = "Jogador não autenticado." });
                return badResp;
            }

            string friendCode = await GetOrGenerateFriendCodeAsync(playFabId);

            if (friendCode == null)
            {
                var badResp = req.CreateResponse(HttpStatusCode.InternalServerError);
                await badResp.WriteAsJsonAsync(new { Success = false, ErrorMessage = "Falha ao gerar Friend Code único." });
                return badResp;
            }

            var successResp = req.CreateResponse(HttpStatusCode.OK);
            await successResp.WriteAsJsonAsync(new { Success = true, FriendCode = friendCode });
            return successResp;
        }

        [Function("GetBootstrapData")]
        public async Task<HttpResponseData> GetBootstrapData(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            _logger.LogInformation("GetBootstrapData function triggered.");

            var (playFabId, context, _) = await AuthHelper.GetContextAsync(req);
            
            if (string.IsNullOrEmpty(playFabId))
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { Success = false, ErrorMessage = "Jogador não autenticado." });
                return badResp;
            }

            // 1. Friend Code
            string friendCode = await GetOrGenerateFriendCodeAsync(playFabId);

            // 2. Economy
            var ecoData = await GetEconomyDataAsync(playFabId);

            // 3. Server Time
            string serverTime = DateTime.UtcNow.ToString("O");

            var successResp = req.CreateResponse(HttpStatusCode.OK);
            await successResp.WriteAsJsonAsync(new { 
                Success = true, 
                FriendCode = friendCode,
                Economy = ecoData,
                ServerTime = serverTime
            });
            return successResp;
        }

        private async Task<string> GetOrGenerateFriendCodeAsync(string playFabId)
        {
            // 1. Verificar se o jogador já tem FriendCode
            var existingData = await _serverApi.GetUserDataAsync(new GetUserDataRequest {
                PlayFabId = playFabId,
                Keys = new List<string> { "FriendCode" }
            });

            if (existingData.Result.Data != null && existingData.Result.Data.ContainsKey("FriendCode"))
            {
                return existingData.Result.Data["FriendCode"].Value;
            }

            // 2. Gerar código único
            string friendCode = null;
            bool isUnique = false;
            int attempts = 0;
            
            while (!isUnique && attempts < 10)
            {
                attempts++;
                friendCode = rng.Next(1000000, 9999999).ToString();
                
                var indexCheck = await _serverApi.GetTitleDataAsync(new GetTitleDataRequest {
                    Keys = new List<string> { $"FC_{friendCode}" }
                });
                
                if (indexCheck.Result.Data == null || !indexCheck.Result.Data.ContainsKey($"FC_{friendCode}"))
                {
                    isUnique = true;
                }
            }

            if (!isUnique)
            {
                return null;
            }

            // 3. Salvar no índice Title Data (FC_1234567 -> PlayFabId)
            await _serverApi.SetTitleDataAsync(new SetTitleDataRequest {
                Key = $"FC_{friendCode}",
                Value = playFabId
            });

            // 4. Salvar no UserData (Public Profile)
            await _serverApi.UpdateUserDataAsync(new UpdateUserDataRequest {
                PlayFabId = playFabId,
                Data = new Dictionary<string, string> { { "FriendCode", friendCode } },
                Permission = UserDataPermission.Public
            });

            return friendCode;
        }

        private async Task<EconomyFunctions.EconomyData> GetEconomyDataAsync(string playFabId)
        {
            var dataResult = await _serverApi.GetUserInternalDataAsync(new GetUserDataRequest
            {
                PlayFabId = playFabId,
                Keys = new List<string> { "Economy" }
            });

            if (dataResult.Result.Data != null && dataResult.Result.Data.ContainsKey("Economy"))
            {
                return JsonConvert.DeserializeObject<EconomyFunctions.EconomyData>(dataResult.Result.Data["Economy"].Value);
            }
            
            return new EconomyFunctions.EconomyData { Money = 100, StarMaps = 0, Stardust = 0 };
        }
    }
}
