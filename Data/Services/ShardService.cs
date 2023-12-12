using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Kafe.Media;
using Kafe.Media.Services;
using Marten;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public class ShardService
{
    private readonly IDocumentSession db;
    private readonly StorageService storageService;
    private readonly IMediaService mediaService;
    private readonly IImageService imageService;

    public ShardService(
        IDocumentSession db,
        StorageService storageService,
        IMediaService mediaService,
        IImageService imageService)
    {
        this.db = db;
        this.storageService = storageService;
        this.mediaService = mediaService;
        this.imageService = imageService;
    }

    public async Task<IShardEntity?> Load(Hrib id, CancellationToken token = default)
    {
        var shardKind = await GetShardKind(id, token);
        if (shardKind == ShardKind.Unknown)
        {
            return null;
        }

        IShardEntity? shard = shardKind switch
        {
            ShardKind.Video => await db.LoadAsync<VideoShardInfo>(id.Value, token),
            ShardKind.Image => await db.LoadAsync<ImageShardInfo>(id.Value, token),
            ShardKind.Subtitles => await db.LoadAsync<SubtitlesShardInfo>(id.Value, token),
            _ => throw new NotSupportedException($"ShardKind '{shardKind}' is not supported.")
        };
        return shard;
    }

    public async Task<Hrib?> Create(
        ShardKind kind,
        Hrib artifactId,
        Stream stream,
        string mimeType,
        Hrib? shardId = null,
        CancellationToken token = default)
    {
        return kind switch
        {
            ShardKind.Video => await CreateVideo(artifactId, stream, mimeType, shardId, token),
            ShardKind.Image => await CreateImage(artifactId, stream, shardId, token),
            ShardKind.Subtitles => await CreateSubtitles(artifactId, stream, shardId, token),
            _ => throw new NotSupportedException($"Creation of '{kind}' shards is not supported yet.")
        };
    }

    private async Task<Hrib?> CreateVideo(
        Hrib artifactId,
        Stream videoStream,
        string mimeType,
        Hrib? shardId = null,
        CancellationToken token = default)
    {
        if (mimeType != Const.MatroskaMimeType && mimeType != Const.Mp4MimeType)
        {
            throw new ArgumentException($"Only '{Const.MatroskaMimeType}' and '{Const.Mp4MimeType}' video container " +
                $"formats are supported.");
        }

        shardId ??= Hrib.Create();
        var originalFileExtension = mimeType == Const.MatroskaMimeType
            ? Const.MatroskaFileExtension
            : Const.Mp4FileExtension;
        if (!await storageService.TryStoreShard(
            ShardKind.Video,
            shardId,
            videoStream,
            Const.OriginalShardVariant,
            originalFileExtension,
            token))
        {
            throw new InvalidOperationException($"Failed to store shard '{shardId}'.");
        }

        if (!storageService.TryGetFilePath(
            ShardKind.Video,
            shardId,
            Const.OriginalShardVariant,
            out var shardFilePath))
        {
            throw new ArgumentException("The shard stream could not be opened just after being saved.");
        }

        var mediaInfo = await mediaService.GetInfo(shardFilePath, token);

        var created = new VideoShardCreated(
            ShardId: shardId.Value,
            CreationMethod: CreationMethod.Api,
            ArtifactId: artifactId.Value,
            OriginalVariantInfo: mediaInfo);
        db.Events.StartStream<VideoShardInfo>(created.ShardId, created);
        await db.SaveChangesAsync(token);

        return created.ShardId;
    }

    private async Task<Hrib?> CreateImage(
        Hrib artifactId,
        Stream imageStream,
        Hrib? shardId = null,
        CancellationToken token = default)
    {
        var imageInfo = await imageService.GetInfo(imageStream, token);

        imageStream.Seek(0, SeekOrigin.Begin);

        shardId ??= Hrib.Create();

        var created = new ImageShardCreated(
            ShardId: shardId.Value,
            CreationMethod: CreationMethod.Api,
            ArtifactId: artifactId.Value,
            OriginalVariantInfo: imageInfo);

        if (!await storageService.TryStoreShard(
            ShardKind.Image,
            created.ShardId,
            imageStream,
            Const.OriginalShardVariant,
            imageInfo.FileExtension,
            token))
        {
            throw new InvalidOperationException("The image could not be stored.");
        }

        db.Events.StartStream<VideoShardInfo>(created.ShardId, created);

        await db.SaveChangesAsync(token);
        return created.ShardId;
    }

    private async Task<Hrib?> CreateSubtitles(
        Hrib artifactId,
        Stream subtitlesStream,
        Hrib? shardId = null,
        CancellationToken token = default)
    {
        var mediaInfo = await mediaService.GetInfo(subtitlesStream, token);

        subtitlesStream.Seek(0, SeekOrigin.Begin);

        if (mediaInfo.SubtitleStreams.IsDefaultOrEmpty)
        {
            throw new ArgumentException("The file contains no subtitle streams.");
        }

        if (mediaInfo.SubtitleStreams.Length > 1)
        {
            throw new ArgumentException("The file contains more than one subtitle stream.");
        }

        var ssInfo = mediaInfo.SubtitleStreams.Single();
        var info = new SubtitlesInfo(
            FileExtension: FFmpegFormat.GetFileExtension(mediaInfo.FormatName) ?? Const.InvalidFileExtension,
            MimeType: FFmpegFormat.GetMimeType(mediaInfo.FormatName) ?? Const.InvalidMimeType,
            Language: ssInfo.Language,
            Codec: ssInfo.Codec,
            Bitrate: ssInfo.Bitrate,
            IsCorrupted: mediaInfo.IsCorrupted);

        shardId ??= Hrib.Create();

        var created = new SubtitlesShardCreated(
            ShardId: shardId.Value,
            CreationMethod: CreationMethod.Api,
            ArtifactId: artifactId.Value,
            OriginalVariantInfo: info);

        if (!await storageService.TryStoreShard(
            ShardKind.Subtitles,
            shardId,
            subtitlesStream,
            Const.OriginalShardVariant,
            info.FileExtension,
            token))
        {
            throw new InvalidOperationException("The subtitles could not be stored.");
        }

        db.Events.StartStream<SubtitlesShardInfo>(created.ShardId, created);

        await db.SaveChangesAsync(token);
        return created.ShardId;
    }

    public async Task<Stream> OpenStream(Hrib id, string? variant, CancellationToken token = default)
    {
        variant = SanitizeVariantName(variant);
        var shardKind = await GetShardKind(id, token);
        if (!storageService.TryOpenShardStream(shardKind, id, variant, out var stream, out _))
        {
            throw new ArgumentException($"A shard stream for the '{id}' shard could not be opened.");
        }

        return stream;
    }

    public async Task<ShardKind> GetShardKind(Hrib id, CancellationToken token = default)
    {
        var firstEvent = (await db.Events.FetchStreamAsync(id.Value, 1, token: token)).SingleOrDefault();
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

    public async Task<VariantInfo?> GetShardVariantMediaType(
        Hrib id,
        string? variant,
        CancellationToken token = default)
    {
        variant = SanitizeVariantName(variant);

        // TODO: Get rid of this switch.
        var shard = await Load(id, token);
        if (shard is null)
        {
            return null;
        }

        return shard.Kind switch
        {
            ShardKind.Video => ((VideoShardInfo)shard).Variants.TryGetValue(variant, out var media)
                ? new VariantInfo(
                    ShardId: shard.Id,
                    Variant: variant,
                    FileExtension: media.FileExtension,
                    MimeType: media.MimeType)
                : null,
            ShardKind.Image => ((ImageShardInfo)shard).Variants.TryGetValue(variant, out var image)
                ? new VariantInfo(
                    ShardId: shard.Id,
                    Variant: variant,
                    FileExtension: image.FileExtension,
                    MimeType: image.MimeType)
                : null,
            ShardKind.Subtitles => ((SubtitlesShardInfo)shard).Variants.TryGetValue(variant, out var image)
                ? new VariantInfo(
                    ShardId: shard.Id,
                    Variant: variant,
                    FileExtension: image.FileExtension,
                    MimeType: image.MimeType)
                : null,
            _ => throw new NotImplementedException()
        }; ;
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
