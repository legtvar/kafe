using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Kafe.Media;
using Marten;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class DefaultShardService : IShardService
{
    private readonly IDocumentSession db;
    private readonly IOptions<StorageOptions> storageOptions;
    private readonly IMediaService mediaService;

    public DefaultShardService(
        IDocumentSession db,
        IOptions<StorageOptions> storageOptions,
        IMediaService mediaService)
    {
        this.db = db;
        this.storageOptions = storageOptions;
        this.mediaService = mediaService;

        if (storageOptions.Value.VideoShardsDirectory is null
            || !Directory.Exists(storageOptions.Value.VideoShardsDirectory))
        {
            throw new ArgumentException($"VideoShard directory '{storageOptions.Value.VideoShardsDirectory}' is not " +
                "configured or does not exist.");
        }
    }

    public async Task<ShardDetailBaseDto?> Load(Hrib id, CancellationToken token = default)
    {
        var shardKind = await GetShardKind(id, token);
        ShardInfoBase? shard = shardKind switch
        {
            ShardKind.Video => await db.LoadAsync<VideoShardInfo>(id, token),
            ShardKind.Image => await db.LoadAsync<ImageShardInfo>(id, token),
            ShardKind.Subtitles => await db.LoadAsync<SubtitlesShardInfo>(id, token),
            _ => throw new NotSupportedException($"ShardKind '{shardKind}' is not supported.")
        };

        if (shard is null)
        {
            return null;
        }

        return TransferMaps.ToShardDetailDto(shard);
    }

    public async Task<Hrib?> Create(
        ShardCreationDto dto,
        string mimeType,
        Stream stream,
        CancellationToken token = default)
    {
        var artifact = await db.LoadAsync<ArtifactInfo>(dto.ArtifactId, token);
        if (artifact is null)
        {
            throw new ArgumentException($"Artifact '{dto.ArtifactId}' does not exist.");
        }

        return dto.Kind switch
        {
            ShardKind.Video => await CreateVideo(dto, mimeType, stream, token),
            _ => throw new NotSupportedException($"Creation of '{dto.Kind}' shards is not supported yet.")
        };
    }

    private async Task<Hrib?> CreateVideo(
        ShardCreationDto dto,
        string mimeType,
        Stream videoStream,
        CancellationToken token = default)
    {
        if (mimeType != Const.MatroskaMimeType && mimeType != Const.Mp4MimeType)
        {
            throw new ArgumentException($"Only '{Const.MatroskaMimeType}' and '{Const.Mp4MimeType}' video container " +
                $"formats are supported.");
        }

        var videoShardsDir = new DirectoryInfo(storageOptions.Value.VideoShardsDirectory!);
        if (!videoShardsDir.Exists)
        {
            throw new InvalidOperationException($"VideoShard directory '{videoShardsDir.FullName}' " +
                $"does not exist.");
        }

        var shardId = Hrib.Create();
        var shardDir = videoShardsDir.CreateSubdirectory(shardId);
        var originalFileExtension = mimeType == Const.MatroskaMimeType
            ? Const.MatroskaFileExtension
            : Const.Mp4FileExtension;
        var originalPath = Path.Combine(shardDir.FullName, $"{Const.OriginalShardVariant}{originalFileExtension}");
        using var originalStream = new FileStream(originalPath, FileMode.Create, FileAccess.Write);
        await videoStream.CopyToAsync(originalStream, token);

        var mediaInfo = await mediaService.GetInfo(originalPath, token);

        var created = new VideoShardCreated(
            ShardId: shardId,
            CreationMethod: CreationMethod.Api,
            ArtifactId: dto.ArtifactId,
            OriginalVariant: new(
                Name: Const.OriginalShardVariant,
                Info: mediaInfo));
        db.Events.StartStream<VideoShardInfo>(created.ShardId, created);

        await db.SaveChangesAsync(token);
        return shardId;
    }

    public async Task<Stream> OpenStream(Hrib id, string variant, CancellationToken token = default)
    {
        var shardKind = await GetShardKind(id, token);

        var shardKindDir = storageOptions.Value.GetShardDirectory(shardKind);
        if (string.IsNullOrEmpty(shardKindDir))
        {
            throw new InvalidOperationException($"Storage directory for ShardKind '{shardKind}' is not set.");
        }

        var shardDir = new DirectoryInfo(Path.Combine(shardKindDir, id));
        if (!shardDir.Exists)
        {
            throw new ArgumentException($"Shard directory '{id}' could not be found.");
        }

        var variantFiles = shardDir.GetFiles($"{variant}.*");
        if (variantFiles.Length == 0)
        {
            throw new ArgumentException($"The '{variant}' variant of shard '{id}' could not be found.");
        }
        else if (variantFiles.Length > 1)
        {
            throw new ArgumentException($"The '{variant}' variant of shard '{id}' has multiple source " +
                "files. This is probably a bug.");
        }

        var variantFile = variantFiles.Single();
        return variantFile.OpenRead();
    }

    public async Task<ShardKind> GetShardKind(Hrib id, CancellationToken token = default)
    {
        var firstEvent = (await db.Events.FetchStreamAsync(id, 1, token: token)).SingleOrDefault();
        if (firstEvent is null)
        {
            return ShardKind.Invalid;
        }

        return ((IShardCreated)firstEvent.Data).GetShardKind();
    }
}
