using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Kafe.Media;
using Marten;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class DefaultArtifactService : IArtifactService
{
    private readonly IDocumentSession db;
    private readonly IOptions<StorageOptions> storageOptions;
    private readonly IMediaService mediaService;

    public DefaultArtifactService(
        IDocumentSession db,
        IOptions<StorageOptions> storageOptions,
        IMediaService mediaService)
    {
        this.db = db;
        this.storageOptions = storageOptions;
        this.mediaService = mediaService;
        if (storageOptions.Value.ArtifactDirectory is null
            || !Directory.Exists(storageOptions.Value.ArtifactDirectory))
        {
            throw new ArgumentException($"Artifact directory '{storageOptions.Value.ArtifactDirectory}' is not " +
                "configured or does not exist.");
        }
    }

    public async Task<ArtifactDetailDto?> Load(Hrib id, CancellationToken token = default)
    {
        var artifact = await db.LoadAsync<ArtifactDetail>(id, token);
        if (artifact is null)
        {
            return null;
        }

        return TransferMaps.ToArtifactDetailDto(artifact);
    }

    public async Task<ImmutableArray<ArtifactDetailDto?>> LoadMany(IEnumerable<Hrib> ids, CancellationToken token = default)
    {
        var artifacts = await db.LoadManyAsync<ArtifactDetail>(token, ids.Cast<string>());
        return artifacts.Select(a => a is null ? null : TransferMaps.ToArtifactDetailDto(a))
            .ToImmutableArray();
    }

    public async Task<Hrib> Create(ArtifactCreationDto dto, CancellationToken token = default)
    {
        var created = new ArtifactCreated(
            ArtifactId: Hrib.Create(),
            CreationMethod: CreationMethod.Api,
            Name: dto.Name);
        db.Events.StartStream<ArtifactInfo>(created.ArtifactId, created);

        if (dto.ContainingProject is not null)
        {
            var artifactAdded = new ProjectArtifactAdded(
                ProjectId: dto.ContainingProject,
                ArtifactId: created.ArtifactId);
            var projectStream = await db.Events.FetchForWriting<ProjectInfo>(dto.ContainingProject, token);
            projectStream.AppendOne(artifactAdded);
        }

        await db.SaveChangesAsync(token);
        return created.ArtifactId;
    }

    public async Task<Hrib?> AddVideoShard(
        Hrib artifactId,
        string mimeType,
        Stream videoStream,
        CancellationToken token = default)
    {
        if (mimeType != Const.MatroskaMimeType && mimeType != Const.Mp4MimeType)
        {
            throw new ArgumentException($"Only '{Const.MatroskaMimeType}' and '{Const.Mp4MimeType}' video container " +
                $"formats are supported.");
        }

        var artifact = await db.LoadAsync<ArtifactInfo>(artifactId, token);
        if (artifact is null)
        {
            throw new ArgumentException($"Artifact '{artifactId}' does not exist.");
        }

        var artifactDir = new DirectoryInfo(Path.Combine(storageOptions.Value.ArtifactDirectory!, artifactId));
        if (!artifactDir.Exists)
        {
            artifactDir.Create();
        }

        var shardId = Hrib.Create();
        var shardDir = artifactDir.CreateSubdirectory(shardId);
        var originalFileExtension = mimeType == Const.MatroskaMimeType
            ? Const.MatroskaFileExtension
            : Const.Mp4FileExtension;
        var originalPath = Path.Combine(shardDir.FullName, $"{Const.OriginalShardVariant}.{originalFileExtension}");
        using var originalStream = new FileStream(originalPath, FileMode.Create, FileAccess.Write);
        await videoStream.CopyToAsync(originalStream, token);

        var mediaInfo = await mediaService.GetInfo(originalPath, token);

        var created = new VideoShardCreated(
            ShardId: shardId,
            CreationMethod: CreationMethod.Api,
            ArtifactId: artifactId,
            OriginalVariant: new(
                Name: Const.OriginalShardVariant,
                Info: mediaInfo));
        db.Events.StartStream<VideoShardInfo>(created.ShardId, created);

        await db.SaveChangesAsync(token);
        return shardId;
    }
}
