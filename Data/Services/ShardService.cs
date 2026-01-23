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

public class ShardService(
    IDocumentSession db,
    StorageService storageService,
    IHttpClientFactory httpClientFactory,
    ShardAnalysisFactory analysisFactory,
    FileExtensionMimeMap extMimeMap,
    KafeObjectFactory kafeObjectFactory
)
{
    public async Task<Err<ShardInfo>> Load(Hrib id, CancellationToken token = default)
    {
        return await db.KafeLoadAsync<ShardInfo>(id, token);
    }

    public async Task<Err<ImmutableArray<ShardInfo>>> LoadMany(IReadOnlyList<Hrib> ids, CancellationToken ct = default)
    {
        return await db.KafeLoadManyAsync<ShardInfo>(ids, ct);
    }

    public async Task<Err<ShardInfo>> Create(
        Type shardType,
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
            return Err.Fail(new BadMimeTypeDiagnostic(mimeType));
        }

        var tmpPath = await storageService.StoreTemporaryShard(shardId, stream, fileExtension, token);
        var analysis = await analysisFactory.Create(shardType, tmpPath, mimeType, token);
        if (!analysis.IsSuccessful)
        {
            await storageService.DeleteTemporaryShard(shardId, token);
            return Err.Fail(new ShardAnalysisFailureDiagnostic(shardType));
        }

        var created = new ShardCreated(
            ShardId: shardId.ToString(),
            // TODO: Allows setting a shard's name
            Name: null,
            CreationMethod: CreationMethod.Api,
            FileLength: stream.Length,
            UploadFilename: uploadFilename,
            MimeType: analysis.MimeType ?? mimeType,
            Payload: kafeObjectFactory.Wrap(analysis.Payload)
        );

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

    public async Task<Err<Stream>> OpenStream(Hrib id, string? variant, CancellationToken token = default)
    {
        variant = SanitizeVariantName(variant);
        var shardErr = await Load(id, token);
        if (shardErr.HasError)
        {
            return shardErr.Diagnostic;
        }

        var shard = shardErr.Value;

        if (!storageService.TryOpenShardStream(shard.Payload.GetType(), id, variant, out var stream, out _))
        {
            throw new ArgumentException($"A shard stream for the '{id}' shard could not be opened.");
        }

        return stream;
    }

    private static string SanitizeVariantName(string? variant)
    {
        // "original" is always the default
        variant ??= Const.OriginalShardVariant;

        // ignore any file extension
        variant = Path.GetFileNameWithoutExtension(variant);

        return variant;
    }
}
