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
using System.Security.Cryptography;
using System.Text;

namespace CelestialCross.Backend
{
    public class ChatAuthFunctions
    {
        private readonly ILogger _logger;

        public ChatAuthFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ChatAuthFunctions>();
        }

        [Function("GetChatToken")]
        public async Task<HttpResponseData> GetChatToken(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            _logger.LogInformation("GetChatToken function triggered.");

            var (playFabId, context, _) = await AuthHelper.GetContextAsync(req);
            
            if (string.IsNullOrEmpty(playFabId))
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { Success = false, ErrorMessage = "Jogador não autenticado." });
                return badResp;
            }

            // O Photon Chat requer que o client envie um token que o servidor de auth (nós) gerou.
            // Para isso, precisamos usar o App Secret do Photon Chat configurado no Azure.
            string photonAppId = Environment.GetEnvironmentVariable("PHOTON_CHAT_APP_ID");
            string photonSecret = Environment.GetEnvironmentVariable("PHOTON_CHAT_SECRET");

            if (string.IsNullOrEmpty(photonSecret))
            {
                _logger.LogError("PHOTON_CHAT_SECRET não configurado.");
                // Retornamos um mock por agora, caso o usuário ainda não tenha configurado
                var mockResp = req.CreateResponse(HttpStatusCode.OK);
                await mockResp.WriteAsJsonAsync(new { Success = true, Token = $"mock_token_for_{playFabId}" });
                return mockResp;
            }

            // Para uma autenticação Custom Authentication no Photon,
            // geralmente você devolve uma estrutura JSON que o Photon entende quando ele faz o callback pro seu servidor.
            // MAS se for gerar um JWT ou token para passar direto pro client usar, a lógica fica aqui.
            // Assumiremos que geramos um JWT simples assinado com a secret.
            
            // ATENÇÃO: Na prática, Custom Authentication no Photon funciona com o cliente 
            // enviando credenciais (ex: session ticket do PlayFab) para o Photon, 
            // e o Photon fazendo um GET/POST no nosso Azure para validar.
            // Vamos implementar aqui um token simples gerado pelo nosso server, que o cliente envia pro Photon 
            // no campo "Token" ou custom auth data.
            
            string token = GenerateSimpleToken(playFabId, photonSecret);

            var okResp = req.CreateResponse(HttpStatusCode.OK);
            await okResp.WriteAsJsonAsync(new { Success = true, Token = token });
            return okResp;
        }

        private string GenerateSimpleToken(string playFabId, string secret)
        {
            // Cria um token HMAC SHA256 simples
            string payload = $"{playFabId}:{DateTime.UtcNow.Ticks}";
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                string signature = Convert.ToBase64String(hash);
                return $"{payload}:{signature}";
            }
        }
    }
}
