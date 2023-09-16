using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Kafe.Data.Options;
using Kafe.Data.Services;
using Kafe.Media;
using Kafe.Media.Services;
using Marten;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class DefaultShardService : IShardService
{
    private readonly IDocumentSession db;
    private readonly IStorageService storageService;
    private readonly IMediaService mediaService;
    private readonly IImageService imageService;
    private readonly IUserProvider userProvider;

    public DefaultShardService(
        IDocumentSession db,
        IStorageService storageService,
        IMediaService mediaService,
        IImageService imageService,
        IUserProvider userProvider)
    {
        this.db = db;
        this.storageService = storageService;
        this.mediaService = mediaService;
        this.imageService = imageService;
        this.userProvider = userProvider;
    }

    public async Task<ShardDetailBaseDto?> Load(Hrib id, CancellationToken token = default)
    {
        var shardKind = await GetShardKind(id, token);
        if (shardKind == ShardKind.Unknown)
        {
            return null;
        }

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

        // await CheckAccess(shard.ArtifactId, token);

        return TransferMaps.ToShardDetailDto(shard);
    }

    public async Task<Hrib?> Create(
        ShardCreationDto dto,
        Stream stream,
        string mimeType,
        CancellationToken token = default)
    {
        await CheckAccess(dto.ArtifactId, token);

        return dto.Kind switch
        {
            ShardKind.Video => await CreateVideo(dto, stream, mimeType, token),
            ShardKind.Image => await CreateImage(dto, stream, token),
            ShardKind.Subtitles => await CreateSubtitles(dto, stream, token),
            _ => throw new NotSupportedException($"Creation of '{dto.Kind}' shards is not supported yet.")
        };
    }

    private async Task<Hrib?> CreateVideo(
        ShardCreationDto dto,
        Stream videoStream,
        string mimeType,
        CancellationToken token = default)
    {
        if (mimeType != Const.MatroskaMimeType && mimeType != Const.Mp4MimeType)
        {
            throw new ArgumentException($"Only '{Const.MatroskaMimeType}' and '{Const.Mp4MimeType}' video container " +
                $"formats are supported.");
        }

        var shardId = Hrib.Create();
        var originalFileExtension = mimeType == Const.MatroskaMimeType
            ? Const.MatroskaFileExtension
            : Const.Mp4FileExtension;
        if (!await storageService.TryStoreShard(
            dto.Kind,
            shardId,
            videoStream,
            Const.OriginalShardVariant,
            originalFileExtension,
            token))
        {
            throw new InvalidOperationException($"Failed to store shard '{shardId}'.");
        }

        if (!storageService.TryGetFilePath(
            dto.Kind,
            shardId,
            Const.OriginalShardVariant,
            out var shardFilePath))
        {
            throw new ArgumentException("The shard stream could not be opened just after being saved.");
        }

        var mediaInfo = await mediaService.GetInfo(shardFilePath, token);

        var created = new VideoShardCreated(
            ShardId: shardId,
            CreationMethod: CreationMethod.Api,
            ArtifactId: dto.ArtifactId,
            OriginalVariantInfo: mediaInfo);
        db.Events.StartStream<VideoShardInfo>(created.ShardId, created);
        await db.SaveChangesAsync(token);

        return created.ShardId;
    }

    private async Task<Hrib?> CreateImage(
        ShardCreationDto dto,
        Stream imageStream,
        CancellationToken token = default)
    {
        var imageInfo = await imageService.GetInfo(imageStream, token);

        imageStream.Seek(0, SeekOrigin.Begin);

        var created = new ImageShardCreated(
            ShardId: Hrib.Create(),
            CreationMethod: CreationMethod.Api,
            ArtifactId: dto.ArtifactId,
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
        ShardCreationDto dto,
        Stream subtitlesStream,
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

        var created = new SubtitlesShardCreated(
            ShardId: Hrib.Create(),
            CreationMethod: CreationMethod.Api,
            ArtifactId: dto.ArtifactId,
            OriginalVariantInfo: info);

        if (!await storageService.TryStoreShard(
            ShardKind.Subtitles,
            created.ShardId,
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
        var firstEvent = (await db.Events.FetchStreamAsync(id, 1, token: token)).SingleOrDefault();
        if (firstEvent is null)
        {
            return ShardKind.Unknown;
        }

        return ((IShardCreated)firstEvent.Data).GetShardKind();
    }

    public async Task<ShardVariantMediaTypeDto?> GetShardVariantMediaType(
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
            ShardKind.Video => ((VideoShardDetailDto)shard).Variants.TryGetValue(variant, out var media)
                ? new ShardVariantMediaTypeDto(
                    ShardId: shard.Id,
                    Variant: variant,
                    FileExtension: media.FileExtension,
                    MimeType: media.MimeType)
                : null,
            ShardKind.Image => ((ImageShardDetailDto)shard).Variants.TryGetValue(variant, out var image)
                ? new ShardVariantMediaTypeDto(
                    ShardId: shard.Id,
                    Variant: variant,
                    FileExtension: image.FileExtension,
                    MimeType: image.MimeType)
                : null,
            ShardKind.Subtitles => ((SubtitlesShardDetailDto)shard).Variants.TryGetValue(variant, out var image)
                ? new ShardVariantMediaTypeDto(
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

    private async Task CheckAccess(Hrib artifactId, CancellationToken token)
    {
        var artifact = await db.LoadAsync<ArtifactDetail>(artifactId, token);
        if (artifact is null)
        {
            throw new IndexOutOfRangeException($"Artifact '{artifactId}' does not exist.");
        }

        await CheckAccess(artifact, token);
    }

    private async Task CheckAccess(ArtifactDetail artifact, CancellationToken token)
    {
        if (userProvider.IsAdministrator())
        {
            return;
        }

        if (artifact.ContainingProjectIds.Length == 0)
        {
            throw new UnauthorizedAccessException();
        }

        var containingProjects = artifact.ContainingProjectIds.Select(p => (string)p).ToImmutableArray();
        var projectCount = await db.Query<ProjectInfo>()
            .Where(p => containingProjects.Contains(p.Id))
            .WhereCanRead(userProvider)
            .CountAsync(token);
        if (projectCount == 0)
        {
            throw new UnauthorizedAccessException();
        }
    }
}
