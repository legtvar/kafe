using Kafe.Pigeons.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.WebHost.UseKestrel();

var app = builder.Build();

// Map endpoints
app.MapPigeonsEndpoint();

app.Run();
