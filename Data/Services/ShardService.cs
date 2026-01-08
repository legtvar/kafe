using Kafe.Core.Diagnostics;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;

namespace Kafe.Data.Services;

public class ShardService
{
    private readonly IDocumentSession db;
    private readonly StorageService storageService;
    private readonly IMediaService mediaService;
    private readonly IImageService imageService;
    private readonly IPigeonsTestQueue pigeonsQueue;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ShardAnalysisFactory analysisFactory;
    private readonly FileExtensionMimeMap extMimeMap;
    private readonly KafeObjectFactory kafeObjectFactory;
    private readonly DiagnosticFactory diagnosticFactory;

    public ShardService(
        IDocumentSession db,
        StorageService storageService,
        IMediaService mediaService,
        IImageService imageService,
        IPigeonsTestQueue pigeonsQueue,
        IHttpClientFactory httpClientFactory,
        ShardAnalysisFactory analysisFactory,
        FileExtensionMimeMap extMimeMap,
        KafeObjectFactory kafeObjectFactory,
        DiagnosticFactory diagnosticFactory
    )
    {
        this.db = db;
        this.storageService = storageService;
        this.mediaService = mediaService;
        this.imageService = imageService;
        this.pigeonsQueue = pigeonsQueue;
        this.httpClientFactory = httpClientFactory;
        this.analysisFactory = analysisFactory;
        this.extMimeMap = extMimeMap;
        this.kafeObjectFactory = kafeObjectFactory;
        this.diagnosticFactory = diagnosticFactory;
    }

    public async Task<ShardInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<ShardInfo>(id.ToString(), token);
    }

    public async Task<ImmutableArray<T>> LoadMany<T>(IEnumerable<Hrib> ids, CancellationToken ct = default)
        where T : IShardEntity
    {
        return (await db.KafeLoadManyAsync<T>([..ids], ct)).Unwrap();
    }

    public async Task<Hrib?> Create(
        KafeType shardType,
        Stream stream,
        string? uploadFilename,
        string mimeType,
        Hrib? shardId = null,
        CancellationToken token = default
    )
    {
        shardId ??= Hrib.Create();
        stream.Seek(0, SeekOrigin.Begin);

        var fileExtension = extMimeMap.GetFirstFileExtensionFor(mimeType);
        if (fileExtension is null)
        {
            return diagnosticFactory.FromPayload(new BadMimeTypeDiagnostic(mimeType));
        }

        var tmpPath = await storageService.StoreTemporaryShard(shardId, stream, fileExtension, token);
        var analysis = await analysisFactory.Create(shardType, tmpPath, mimeType, token);
        if (!analysis.IsSuccessful)
        {
            await storageService.DeleteTemporaryShard(shardId, token);
            return diagnosticFactory.FromPayload(new ShardAnalysisFailureDiagnostic(shardType));
        }

        var created = new ShardCreated(
            ShardId: shardId.ToString(),
            // TODO: Allows setting a shard's name
            Name: null,
            CreationMethod: CreationMethod.Api,
            FileLength: stream.Length,
            UploadFilename: uploadFilename,
            MimeType: analysis.MimeType ?? mimeType,
            Metadata: kafeObjectFactory.Wrap(analysis.Payload)
        );
        if (created.Metadata.Type != shardType)
        {
            throw new InvalidOperationException($"The '{analysis.ShardAnalyzerName ?? nameof(ShardAnalysisFactory)}'" +
                $" produced a shard of type '{created.Metadata.Type}' but '{shardType}' was required. " +
                "The shard type and/or its analyzers are likely misconfigured.");
        }

        db.Events.KafeStartStream<ShardInfo>(created.ShardId, created);
        await db.SaveChangesAsync(token);
        await storageService.MoveTemporaryShard(
            id: shardId,
            shardType: shardType,
            fileExtension: analysis.FileExtension ?? fileExtension,
            ct: token
        );
        return await db.Events.KafeAggregateRequiredStream<ShardInfo>(shardId, token: token);
    }

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
            out var shardFilePath))
        {
            throw new ArgumentException("The shard stream could not be opened just after being saved.");
        }

        var artifactService = new ArtifactService(db);
        var projectGroupNames = await artifactService.GetArtifactProjectGroupNames(artifactId.ToString(), token);
        if (projectGroupNames.Length != 1)
        {
            throw new InvalidOperationException($"A blend shard must belong to exactly one project group. Found {projectGroupNames.Length}.");
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
            OriginalVariantInfo: blendInfo);

        db.Events.KafeStartStream<BlendShardInfo>(created.ShardId, created);
        await db.SaveChangesAsync(token);

        await pigeonsQueue.EnqueueAsync(shardId);

        return created.ShardId;
    }

    public async Task<BlendShardInfo> UpdateBlend(
        Hrib shardId,
        BlendInfo blendInfo,
        CancellationToken token = default)
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
                && s.Variants[Const.OriginalShardVariant].Error == null)
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
            out var shardFilePath))
        {
            return null;
        }
        var request = new PigeonsTestRequest(
            ShardId: shardId.ToString(),
            HomeworkType: projectGroupNames[0]["iv"] ?? string.Empty,
            Path: shardFilePath);
        var client = httpClientFactory.CreateClient("Pigeons");
        var response = await client.PostAsJsonAsync("/test", request, cancellationToken: ct);
        var content = await response.Content.ReadFromJsonAsync<BlendInfoJsonFormat>(cancellationToken: ct);
        if (content is null)
        {
            throw new InvalidOperationException("Failed to get pigeons test info from pigeons service.");
        }
        return await UpdateBlend(shardId, content.ToBlendInfo(), ct);
    }

    public async Task<Stream> OpenStream(Hrib id, string? variant, CancellationToken token = default)
    {
        variant = SanitizeVariantName(variant);
        var shard = await Load(id, token)
            ?? throw new ArgumentException($"Shard {id} could not be found.");
        if (!storageService.TryOpenShardStream(shard.Payload.Type, id, variant, out var stream, out _))
        {
            throw new ArgumentException($"A shard stream for the '{id}' shard could not be opened.");
        }

        return stream;
    }

    public async Task<ShardKind> GetShardKind(Hrib id, CancellationToken token = default)
    {
        var firstEvent = (await db.Events.FetchStreamAsync(id.ToString(), 1, token: token)).SingleOrDefault();
        if (firstEvent is null)
        {
            return ShardKind.Unknown;
        }

        return ((IShardCreated)firstEvent.Data).GetShardKind();
    }

    public record VariantInfo(
        Hrib ShardId,
        string Variant,
        string? FileExtension,
        string? MimeType
    );

    private static string SanitizeVariantName(string? variant)
    {
        // "original" is always the default
        variant ??= Const.OriginalShardVariant;

        // ignore any file extension
        variant = Path.GetFileNameWithoutExtension(variant);

        return variant;
    }
}
