namespace Kafe.Mate;

public class PigeonsService
{
    private async Task<Hrib?> CreateBlend(
        Hrib artifactId,
        string? fileName,
        Stream blendStream,
        Hrib? shardId = null,
        CancellationToken token = default
    )
    {
        blendStream.Seek(0, SeekOrigin.Begin);
        shardId ??= Hrib.Create();

        if (!await storageService.TryStoreShard(
                ShardKind.Blend,
                shardId,
                blendStream,
                Const.OriginalShardVariant,
                Const.BlendFileExtension,
                token
            ))
        {
            throw new InvalidOperationException("The .blend file could not be stored.");
        }

        if (!storageService.TryGetFilePath(
                ShardKind.Blend,
                shardId,
                Const.OriginalShardVariant,
                out var shardFilePath
            ))
        {
            throw new ArgumentException("The shard stream could not be opened just after being saved.");
        }

        var artifactService = new ArtifactService(db);
        var projectGroupNames = await artifactService.GetArtifactProjectGroupNames(artifactId.ToString(), token);
        if (projectGroupNames.Length != 1)
        {
            throw new InvalidOperationException(
                $"A blend shard must belong to exactly one project group. Found {projectGroupNames.Length}."
            );
        }

        var blendInfo = new BlendInfo(
            FileExtension: Const.BlendFileExtension,
            MimeType: Const.BlendMimeType,
            Tests: null,
            Error: null
        );

        var created = new BlendShardCreated(
            ShardId: shardId.ToString(),
            FileName: fileName,
            CreationMethod: CreationMethod.Api,
            ArtifactId: artifactId.ToString(),
            OriginalVariantInfo: blendInfo
        );

        db.Events.KafeStartStream<BlendShardInfo>(created.ShardId, created);
        await db.SaveChangesAsync(token);

        await pigeonsQueue.EnqueueAsync(shardId);

        return created.ShardId;
    }

    public async Task<BlendShardInfo> UpdateBlend(
        Hrib shardId,
        BlendInfo blendInfo,
        CancellationToken token = default
    )
    {
        var changed = new BlendShardVariantAdded(
            ShardId: shardId.ToString(),
            Name: Const.OriginalShardVariant,
            Info: blendInfo
        );
        db.Events.KafeAppend(changed.ShardId, changed);

        await db.SaveChangesAsync(token);

        return await db.Events.KafeAggregateRequiredStream<BlendShardInfo>(shardId, token: token);
    }

    public async Task<List<Hrib>> GetMissingTestBlends(CancellationToken ct)
    {
        // Fetch all shard IDs that were queued for testing but have not yet been tested
        var blendShards = await db.Query<BlendShardInfo>().ToListAsync();
        var missingTestShards = blendShards
            .Where(s => s.Variants.ContainsKey(Const.OriginalShardVariant)
                && s.Variants[Const.OriginalShardVariant].Tests == null
                && s.Variants[Const.OriginalShardVariant].Error == null
            )
            .Select(s => (Hrib)s.Id);

        return missingTestShards.ToList();
    }

    public async Task<BlendShardInfo?> TestBlend(Hrib shardId, CancellationToken ct)
    {
        var shard = await Load(shardId, ct);
        if (shard == null)
        {
            return null;
        }

        var artifactService = new ArtifactService(db);
        var projectGroupNames = await artifactService.GetArtifactProjectGroupNames(shard.ArtifactId.ToString(), ct);
        if (projectGroupNames.Length < 1)
        {
            return null;
        }

        if (!storageService.TryGetFilePath(
                ShardKind.Blend,
                shard.Id,
                Const.OriginalShardVariant,
                out var shardFilePath
            ))
        {
            return null;
        }

        var request = new PigeonsTestRequest(
            ShardId: shardId.ToString(),
            HomeworkType: projectGroupNames[0]["iv"] ?? string.Empty,
            Path: shardFilePath
        );
        var client = httpClientFactory.CreateClient("Pigeons");
        var response = await client.PostAsJsonAsync("/test", request, cancellationToken: ct);
        var content = await response.Content.ReadFromJsonAsync<BlendInfoJsonFormat>(cancellationToken: ct);
        if (content is null)
        {
            throw new InvalidOperationException("Failed to get pigeons test info from pigeons service.");
        }

        return await UpdateBlend(shardId, content.ToBlendInfo(), ct);
    }

    private static DiagnosticKind StatusToDiagnosticKind(string status)
    {
        switch (status)
        {
            case "INIT":
                return DiagnosticKind.Info;
            case "OK":
                return DiagnosticKind.Info;
            case "SKIPPED":
                return DiagnosticKind.Info;
            case "WARNING":
                return DiagnosticKind.Warning;
            case "ERROR":
                return DiagnosticKind.Error;
            case "CRASHED":
                return DiagnosticKind.Error;
            default:
                return DiagnosticKind.Info;
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
