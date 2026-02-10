using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Kafe.Mate.Diagnostics;
using Marten;
using Marten.Linq;

namespace Kafe.Mate;

public class BlendService(
    StorageService storageService,
    ArtifactService artifactService,
    ShardService shardService,
    IHttpClientFactory httpClientFactory
)
{
    public const string TestEndpoint = "/test";

    public async Task<ImmutableArray<ShardInfo>> GetUntestedBlends(CancellationToken ct)
    {
        var query = shardService.Query(
            new ShardService.ShardFilter(
                ShardPayloadType: typeof(BlendInfo)
            )
        );
        query = (IMartenQueryable<ShardInfo>)query.Where(s =>
            ((BlendInfo)s.Payload.Value).Error == null && ((BlendInfo)s.Payload.Value).Tests == null
        );
        return [..await query.ToListAsync(ct)];
    }

    public async Task<Err<PigeonsTestResponse>> TestBlend(PigeonsTestRequest testRequest, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("Pigeons");
        var response = await client.PostAsJsonAsync(TestEndpoint, testRequest, cancellationToken: ct);
        var content = await response.Content.ReadFromJsonAsync<PigeonsTestResponse>(cancellationToken: ct);
        if (content is null)
        {
            return Err.Fail(new PigeonsFailedToRunDiagnostic("Could not read test results."));
        }

        return content;
    }

    private static DiagnosticSeverity StatusToDiagnosticKind(string status)
    {
        return status switch
        {
            "INIT" or "OK" or "SKIPPED" => DiagnosticSeverity.Info,
            "WARNING" => DiagnosticSeverity.Warning,
            "ERROR" or "CRASHED" => DiagnosticSeverity.Error,
            _ => DiagnosticSeverity.Info
        };
    }


    private static Err<bool> ValidatePigeons(BlendInfo blend)
    {
        var err = new Err<bool>();
        if (blend.Error is not null)
        {
            err = err.Combine(Err.Fail(new PigeonsFailedToRunDiagnostic(blend.Error)));
        }

        var testDiagnostics = (blend.Tests ?? []).Select(t => Err.Fail(
                new PigeonsTestDiagnostic(
                    Label: t.Label,
                    Datablock: t.Datablock,
                    InnerMessage: t.Message,
                    Traceback: t.Traceback
                )
            )
        );
        err = err.Combine(testDiagnostics);
        return err;
    }
}
