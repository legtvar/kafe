using Kafe.Core.Diagnostics;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public class ShardService
{
    private readonly IDocumentSession db;
    private readonly StorageService storageService;
    private readonly ShardAnalysisFactory analysisFactory;
    private readonly FileExtensionMimeMap extMimeMap;
    private readonly KafeObjectFactory kafeObjectFactory;
    private readonly DiagnosticFactory diagnosticFactory;

    public ShardService(
        IDocumentSession db,
        StorageService storageService,
        ShardAnalysisFactory analysisFactory,
        FileExtensionMimeMap extMimeMap,
        KafeObjectFactory kafeObjectFactory,
        DiagnosticFactory diagnosticFactory
    )
    {
        this.db = db;
        this.storageService = storageService;
        this.analysisFactory = analysisFactory;
        this.extMimeMap = extMimeMap;
        this.kafeObjectFactory = kafeObjectFactory;
        this.diagnosticFactory = diagnosticFactory;
    }

    public async Task<ShardInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<ShardInfo>(id.ToString(), token);
    }

    public async Task<Err<ShardInfo>> Create(
        KafeType shardType,
        Stream stream,
        string? uploadFilename,
        string mimeType,
        Hrib? shardId = null,
        CancellationToken token = default)
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
            Metadata: kafeObjectFactory.Wrap(analysis.ShardMetadata)
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

    public async Task<Stream> OpenStream(Hrib id, string? variant, CancellationToken token = default)
    {
        variant = SanitizeVariantName(variant);
        var shard = await Load(id, token)
            ?? throw new ArgumentException($"Shard {id} could not be found.");
        if (!storageService.TryOpenShardStream(shard.Metadata.Type, id, variant, out var stream, out _))
        {
            throw new ArgumentException($"A shard stream for the '{id}' shard could not be opened.");
        }

        return stream;
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
