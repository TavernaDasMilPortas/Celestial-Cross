---
name: azure-functions-playfab
description: Instruções e diretrizes para criar ou atualizar Azure Functions C# focadas no backend do PlayFab no projeto Celestial Cross.
---

# Diretrizes para Azure Functions no PlayFab (Modelo ISOLATED)

Este projeto utiliza o PlayFab como backend e roda funções personalizadas na Microsoft Azure usando o modelo moderno **.NET Isolated** (.NET 8/10).
Sempre que for solicitado a criar, modificar ou publicar uma Azure Function para o PlayFab, siga rigorosamente as diretrizes abaixo.

## 1. Localização do Código
- **NUNCA** crie ou edite arquivos de Azure Functions dentro da pasta `Assets` do Unity. O código de servidor utiliza bibliotecas (como `PlayFabAllSDK` para .NET e referências do Azure Worker) que quebram a compilação local da Unity.
- Todo o código do backend deve estar na pasta raiz do projeto de servidor: `Backend/`.
- Se você criar um novo arquivo `.cs` de backend, salve-o dentro de `Backend/`.

## 2. Padrão de Escrita (Isolated Worker)
Azure Functions isoladas usam classes instanciáveis, Injeção de Dependência, `[Function]` (e não `[FunctionName]`) e os tipos `HttpRequestData` / `HttpResponseData`.

```csharp
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

namespace CelestialCross.Backend
{
    public class MyFunctions
    {
        private readonly ILogger _logger;

        public MyFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<MyFunctions>();
        }

        [Function("NomeDaSuaFuncaoAqui")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req)
        {
            // 1. Ler Corpo da Requisição (PlayFab envia Context)
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic context = JsonConvert.DeserializeObject(requestBody);
            
            // 2. Extrair PlayFabId
            string playFabId = context?.CallerEntityProfile?.Lineage?.MasterPlayerAccountId;
            if (string.IsNullOrEmpty(playFabId))
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { Success = false, ErrorMessage = "Jogador não autenticado." });
                return badResp;
            }

            // 3. Resposta de Sucesso
            var okResp = req.CreateResponse(HttpStatusCode.OK);
            await okResp.WriteAsJsonAsync(new { Success = true });
            return okResp;
        }
    }
}
```

## 3. Publicação (Deploy)
Quando for solicitado a "subir" ou "atualizar" as funções:
1. Informe ao usuário que o processo deve ser feito pelo terminal do sistema dele.
2. Comandos:
   ```bash
   cd Backend
   func azure functionapp publish <NOME_DO_APP_NA_AZURE>
   ```
3. A Azure Function APP deve estar configurada como **DotnetIsolated**.

## 4. Integração na Unity (NetworkGuard)
Sempre que uma nova função for criada na nuvem, ela deve ser chamada na Unity através do **NetworkGuard** para garantir resiliência contra quedas de internet.
