using Kafe.Pigeons.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.WebHost.UseKestrel();

var app = builder.Build();

// Map endpoints
app.MapPigeonsEndpoint();

app.Logger.LogInformation(
    "Temp directory: {TempDirectory}",
    app.Configuration.GetValue<string>("Storage:TempDirectory")
);

app.Run();
