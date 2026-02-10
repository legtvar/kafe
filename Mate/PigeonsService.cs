using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Marten;
using Marten.Linq;

namespace Kafe.Mate;

public class PigeonsService(
    StorageService storageService,
    ArtifactService artifactService,
    ShardService shardService,
    IHttpClientFactory httpClientFactory
)
{
    public const string TestEndpoint = "/test";

    public Task<Err<bool>> UpdateBlend(
        Hrib shardId,
        BlendInfo blendInfo,
        CancellationToken token = default
    )
    {
        return shardService.SetShardPayload(shardId, blendInfo, token);
    }

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

    public async Task<Err<BlendInfo>> TestBlend(Hrib shardId, string? homeworkType, CancellationToken ct)
    {
        var shard = await shardService.Load(shardId, ct);
        if (shard.HasError)
        {
            return shard.Diagnostic;
        }

        var shardUri = storageService.GetShardUri(shardId, typeof(BlendInfo));
        if (shardUri.HasError)
        {
            return shardUri.Diagnostic;
        }

        var request = new PigeonsTestRequest(
            ShardUri: shardUri.Value,
            HomeworkType: homeworkType ?? string.Empty
        );
        var client = httpClientFactory.CreateClient("Pigeons");
        var response = await client.PostAsJsonAsync(TestEndpoint, request, cancellationToken: ct);
        var content = await response.Content.ReadFromJsonAsync<BlendInfo>(cancellationToken: ct);
        if (content is null)
        {
            throw new InvalidOperationException("Failed to get PIGEOnS test results from the service.");
        }

        var updateErr = await UpdateBlend(shardId, content, ct);
        if (updateErr.HasError)
        {
            return updateErr.Diagnostic;
        }

        return (await shardService.Load(shardId, ct)).Select(s => (BlendInfo)s.Payload.Value);
    }

    private static DiagnosticSeverity StatusToDiagnosticKind(string status)
    {
        switch (status)
        {
            case "INIT":
                return DiagnosticSeverity.Info;
            case "OK":
                return DiagnosticSeverity.Info;
            case "SKIPPED":
                return DiagnosticSeverity.Info;
            case "WARNING":
                return DiagnosticSeverity.Warning;
            case "ERROR":
                return DiagnosticSeverity.Error;
            case "CRASHED":
                return DiagnosticSeverity.Error;
            default:
                return DiagnosticSeverity.Info;
        }
    }


    private static IEnumerable<Diagnostic> ValidatePigeons(BlendShardInfo? blend)
    {
        if (blend is null)
        {
            yield break;
        }

        foreach (var variant in blend.Variants.Values)
        {
            if (variant.Error is not null)
            {
                yield return new Diagnostic(
                    Kind: DiagnosticKind.Error,
                    ValidationStage: PigeonsTestStage,
                    Message: LocalizedString.Create(
                        (Const.InvariantCulture, $"PIGEOnS test could not be run: {variant.Error}"),
                        (Const.CzechCulture, $"PIGEOnS test nemohl být spuštěn: {variant.Error}")
                    )
                );
                yield break;
            }

            if (variant.Tests is null)
            {
                yield return MissingPigeonsTestResult;
                yield break;
            }
            foreach (var result in variant.Tests)
            {
                var sb = new System.Text.StringBuilder();
                if (!string.IsNullOrWhiteSpace(blend.FileName))
                {
                    sb.Append(blend.FileName);
                    sb.Append(" - ");
                }
                if (!string.IsNullOrWhiteSpace(result.Label))
                {
                    sb.Append(result.Label);
                }
                // Append additional info if any of the fields are present
                if (!string.IsNullOrWhiteSpace(result.Datablock) ||
                    !string.IsNullOrWhiteSpace(result.Message) ||
                    !string.IsNullOrWhiteSpace(result.Traceback))
                {
                    sb.Append(":");
                }
                else
                {
                    sb.Append(".");
                }
                if (!string.IsNullOrWhiteSpace(result.Datablock))
                {
                    sb.Append(" [");
                    sb.Append(result.Datablock);
                    sb.Append("]");
                }
                if (!string.IsNullOrWhiteSpace(result.Message))
                {
                    sb.Append(" ");
                    sb.Append(result.Message);
                }
                if (!string.IsNullOrWhiteSpace(result.Traceback))
                {
                    sb.Append(" ");
                    sb.Append("Traceback: ");
                    sb.Append(result.Traceback);
                }
                string message = sb.ToString();

                yield return new Diagnostic(
                    Kind: StatusToDiagnosticKind(result.State ?? "UNKNOWN"),
                    ValidationStage: PigeonsTestStage,
                    Message: LocalizedString.Create(
                        (Const.InvariantCulture, message)
                    )
                );
            }
        }
    }
}
