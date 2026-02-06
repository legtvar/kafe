using Kafe.Core.Diagnostics;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using Marten.Linq;
using Marten.Linq.MatchesSql;

namespace Kafe.Data.Services;

public class ShardService(
    IDocumentSession db,
    StorageService storageService,
    ShardAnalysisFactory analysisFactory,
    FileExtensionMimeMap extMimeMap,
    KafeTypeRegistry typeRegistry,
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

        var tmpUri = await storageService.StoreTemporaryShard(shardId, stream, fileExtension, token);
        if (tmpUri.HasError)
        {
            return tmpUri.Diagnostic;
        }

        var analysis = await analysisFactory.Analyze(shardType, tmpUri.Value, mimeType, uploadFilename, token);
        if (!analysis.IsSuccessful)
        {
            storageService.DeleteTemporaryShard(shardId, token);
            return Err.Fail(new ShardAnalysisFailedDiagnostic(shardType));
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
        storageService.MoveTemporaryToArchive(
            tmpShardId: shardId,
            shardType: shardType,
            fileExtension: analysis.FileExtension ?? fileExtension
        );
        return await db.Events.KafeAggregateRequiredStream<ShardInfo>(shardId, token: token);
    }

    public async Task<Err<ShardInfo>> Upsert(
        ShardInfo shard,
        ExistingEntityHandling existingEntityHandling = ExistingEntityHandling.Upsert,
        CancellationToken ct = default
    )
    {
        var idErr = Hrib.TryParseValid(shard.Id, shouldReplaceEmpty: true);
        if (idErr.HasError)
        {
            return idErr.Diagnostic.ForParameter(nameof(shard.Id));
        }

        var id = idErr.Value;

        var shardErr = (await Load(id, ct)).HandleExistingEntity(existingEntityHandling);
        if (shardErr is { HasError: true, Diagnostic.Payload: NotCreatedDiagnostic })
        {
            var created = new ShardCreated(
                ShardId: id.ToString(),
                Name: shard.Name,
                CreationMethod: CreationMethod.Api,
                FileLength: shard.FileLength,
                UploadFilename: shard.UploadFilename,
                MimeType: shard.MimeType,
                Payload: shard.Payload
            );
            db.Events.KafeStartStream<ShardInfo>(id, created);
            await db.SaveChangesAsync(ct);
            shardErr = await db.Events.KafeAggregateStream<ShardInfo>(id, token: ct);
        }

        // NB: This is now only the Update path.
        if (shardErr.HasError)
        {
            return shardErr;
        }

        var existing = shardErr.Value;

        var eventStream = await db.Events.FetchForExclusiveWriting<ShardInfo>(id.ToString(), ct);

        var infoChanged = new ShardInfoChanged(
            ShardId: id.ToString(),
            Name: Const.PreferNew<LocalizedString>(existing.Name, shard.Name),
            FileLength: Const.PreferNew(existing.FileLength, shard.FileLength),
            UploadFilename: Const.PreferNew(existing.UploadFilename, shard.UploadFilename),
            MimeType: Const.PreferNew(existing.MimeType, shard.MimeType)
        );

        if (infoChanged.Name != null || infoChanged.FileLength != null || infoChanged.UploadFilename != null
            || infoChanged.MimeType != null)
        {
            eventStream.AppendOne(infoChanged);
        }

        if (existing.Payload != shard.Payload)
        {
            eventStream.AppendOne(
                new ShardPayloadSet(
                    ShardId: id.ToString(),
                    Payload: shard.Payload,
                    ExistingValueHandling: ExistingValueHandling.OverwriteExisting
                )
            );
        }

        foreach (var removedLink in existing.Links.Except(shard.Links))
        {
            eventStream.AppendOne(
                new ShardLinkRemoved(
                    SourceShardId: id.ToString(),
                    DestinationShardId: removedLink.DestinationId,
                    LinkPayload: removedLink.Payload
                )
            );
        }

        var linksErr = new Err<bool>();

        foreach (var addedLink in shard.Links.Except(existing.Links))
        {
            var destinationShardErr = await Load(addedLink.DestinationId, ct);
            if (destinationShardErr.HasError)
            {
                linksErr.Combine(destinationShardErr.Diagnostic);
                continue;
            }

            eventStream.AppendOne(
                new ShardLinkAdded(
                    SourceShardId: id.ToString(),
                    DestinationShardId: addedLink.DestinationId,
                    LinkPayload: addedLink.Payload
                )
            );
        }

        if (linksErr.HasError)
        {
            linksErr = linksErr.Diagnostic.ForParameter(nameof(shard.Links));
        }

        await db.SaveChangesAsync(ct);

        shardErr = await db.Events.KafeAggregateStream<ShardInfo>(id, token: ct);
        return shardErr.Combine(linksErr.Diagnostic);
    }

    public async Task<Err<bool>> AddShardLink(
        Hrib sourceShardId,
        Hrib destinationShardId,
        IShardLinkPayload payload,
        CancellationToken ct = default
    )
    {
        var dstShardErr = await Load(destinationShardId, ct);
        if (dstShardErr.HasError)
        {
            return dstShardErr.Diagnostic.ForParameter(nameof(dstShardErr));
        }

        var srcShardEventStream = await db.Events.KafeFetchForExclusiveWriting<ShardInfo>(sourceShardId, ct);
        if (srcShardEventStream.HasError)
        {
            return srcShardEventStream.Diagnostic;
        }

        srcShardEventStream.Value.AppendOne(
            new ShardLinkAdded(
                SourceShardId: sourceShardId.ToString(),
                DestinationShardId: destinationShardId.ToString(),
                LinkPayload: kafeObjectFactory.Wrap(payload)
            )
        );

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<Err<bool>> SetShardPayload(
        Hrib shardId,
        IShardPayload payload,
        CancellationToken ct = default
    )
    {
        var shardStream = await db.Events.KafeFetchForExclusiveWriting<ShardInfo>(shardId, ct);
        if (shardStream.HasError)
        {
            return shardStream.Diagnostic;
        }

        shardStream.Value.AppendOne(new ShardPayloadSet(
            ShardId: shardId.ToString(),
            Payload: kafeObjectFactory.Wrap(payload),
            ExistingValueHandling: ExistingValueHandling.OverwriteExisting
        ));
        await db.SaveChangesAsync(ct);
        return true;
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

        if (!storageService.TryOpenShardStream(id, shard.Payload.GetType(), variant, out var stream, out _))
        {
            throw new ArgumentException($"A shard stream for the '{id}' shard could not be opened.");
        }

        return stream;
    }

    public record ShardFilter(
        Type? ShardPayloadType = null,
        TimeSpan? Age = null
    );

    public IMartenQueryable<ShardInfo> Query(ShardFilter? filter = null)
    {
        filter ??= new ShardFilter();
        var query = db.Query<ShardInfo>();
        if (filter.ShardPayloadType is not null)
        {
            var payloadKafeType = typeRegistry.RequireType(filter.ShardPayloadType);
            query = (IMartenQueryable<ShardInfo>)query.Where(s => s.Payload.Type == payloadKafeType);
        }

        if (filter.Age is not null)
        {
            var rangeStart = DateTimeOffset.UtcNow - filter.Age.Value;
            query = (IMartenQueryable<ShardInfo>)query.Where(v => v.CreatedAt > rangeStart);
        }

        return query;
    }

    public record ContainingProjectGroupInfo(
        Hrib Id,
        LocalizedString Name
    );

    public async Task<Err<ImmutableArray<ContainingProjectGroupInfo>>> GetContainingProjectGroups(
        Hrib id,
        CancellationToken ct = default
    )
    {
        var artifactErr = await Load(id, ct);
        if (artifactErr.HasError)
        {
            return artifactErr.Diagnostic;
        }

        var containingProjectGroups = await db.Query<ProjectGroupInfo>()
            .Where(g => g.MatchesSql(
                    $"""
                     EXISTS(
                        SELECT a.* FROM {db.DocumentStore.Options.Schema.For<ProjectInfo>(true)} AS a
                        WHERE a.data ->> '{nameof(ProjectInfo.ProjectGroupId)}' = d.id
                            AND a.data ->> '{nameof(ProjectInfo.ArtifactId)}' = ?
                     """,
                    id.ToString()
                )
            )
            .ToListAsync(ct);

        return containingProjectGroups.DistinctBy(g => g.Id).Select(g => new ContainingProjectGroupInfo(
                Id: Hrib.Parse(g.Id),
                Name: g.Name
            )
        ).ToImmutableArray();
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
