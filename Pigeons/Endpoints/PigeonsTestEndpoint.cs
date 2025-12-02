using Kafe.Pigeons.Services;

namespace Kafe.Pigeons.Endpoints
{
    public static class PigeonsTestEndpoint
    {
        public static void MapPigeonsEndpoint(this WebApplication app)
        {
            app.MapPost("/test", async (RequestData request) =>
            {
                var service = new PigeonsService(app.Configuration.GetValue<string>("Storage:TempDirectory")
                    ?? throw new ArgumentException("The temp directory is not configured."),
                    app.Logger
                );
                var pigeonsInfo = await service.RunPigeonsTest(request.ShardId, Uri.UnescapeDataString(request.Path), request.HomeworkType);

                return Results.Ok(pigeonsInfo);
            });
        }
    }

    public record RequestData(
        string ShardId,
        string HomeworkType,
        string Path
    );
}
