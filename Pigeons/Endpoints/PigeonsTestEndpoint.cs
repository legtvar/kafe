using Kafe.Mate;
using Kafe.Pigeons.Services;

namespace Kafe.Pigeons.Endpoints
{
    public static class PigeonsTestEndpoint
    {
        public static void MapPigeonsEndpoint(this WebApplication app)
        {
            var service = app.Services.GetRequiredService<PigeonsService>();
            app.MapPost(
                "/test",
                async (PigeonsTestRequest request, CancellationToken ct) =>
                {
                    var pigeonsInfo = await service.RunPigeonsTest(request, ct);
                    return Results.Ok(pigeonsInfo);
                }
            );
        }
    }
}
