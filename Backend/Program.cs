using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PlayFab;
using CelestialCross.Backend.Services;
using System;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// DI: Singleton de PlayFab para reutilizar conexões HTTP
builder.Services.AddSingleton<PlayFabApiSettings>(_ => new PlayFabApiSettings
{
    TitleId = Environment.GetEnvironmentVariable("PLAYFAB_TITLE_ID") ?? "SEU_TITLE_ID",
    DeveloperSecretKey = Environment.GetEnvironmentVariable("PLAYFAB_DEV_SECRET_KEY") ?? "SUA_SECRET_KEY"
});
builder.Services.AddSingleton<PlayFabServerInstanceAPI>();

// Warmup: Pré-carrega gamedata.json no startup
GameDataService.Initialize();

builder.Build().Run();
