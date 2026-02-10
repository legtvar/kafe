using Kafe.Data.Options;
using Kafe.Pigeons;
using Kafe.Pigeons.Endpoints;
using Kafe.Pigeons.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<PigeonsService>();
builder.Services.AddOptions<StorageOptions>();
builder.WebHost.UseKestrel();

var app = builder.Build();

// Map endpoints
app.MapPigeonsEndpoint();

app.Logger.LogInformation(
    "Temp directory: {TempDirectory}",
    app.Configuration.GetValue<string>("Storage:TempDirectory")
);

app.Run();
