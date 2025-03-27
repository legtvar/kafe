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

namespace Kafe.Data.Services;

public class ShardService
{
    private readonly IDocumentSession db;
    private readonly StorageService storageService;
    private readonly IMediaService mediaService;
    private readonly IImageService imageService;
    private readonly IPigeonsTestQueue pigeonsQueue;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ShardFactory shardFactory;

    public ShardService(
        IDocumentSession db,
        StorageService storageService,
        IMediaService mediaService,
        IImageService imageService,
        IPigeonsTestQueue pigeonsQueue,
        IHttpClientFactory httpClientFactory,
        ShardFactory shardFactory
    )
    {
        this.db = db;
        this.storageService = storageService;
        this.mediaService = mediaService;
        this.imageService = imageService;
        this.pigeonsQueue = pigeonsQueue;
        this.httpClientFactory = httpClientFactory;
        this.shardFactory = shardFactory;
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
        Hrib artifactId,
        string? fileName,
        Stream stream,
        string mimeType,
        Hrib? shardId = null,
        CancellationToken token = default
    )
    {
        shardId ??= Hrib.Create();
        stream.Seek(0, SeekOrigin.Begin);

        var tmpPath = await storageService.TryStoreTemporaryShard(shardId, stream, token);
        if (tmpPath is null)
        {
            return null;
        }

        await shardFactory.Create(shardType, tmpPath, )

        var shardTypeMetadata = shardTypes.Shards.GetValueOrDefault(shardType)
            ?? throw new ArgumentException($"Shard type '{shardType}' could not be recognized.");
        shardTypeMetadata.
        return kind switch
        {
            ShardKind.Video => await CreateVideo(artifactId, stream, mimeType, shardId, token),
            ShardKind.Image => await CreateImage(artifactId, stream, shardId, token),
            ShardKind.Subtitles => await CreateSubtitles(artifactId, stream, shardId, token),
            ShardKind.Blend => await CreateBlend(artifactId, fileName, stream, shardId, token),
            _ => throw new NotSupportedException($"Creation of '{kind}' shards is not supported yet.")
        };
    }

    private async Task<Hrib?> CreateVideo(
        Hrib artifactId,
        Stream videoStream,
        string mimeType,
        Hrib? shardId = null,
        CancellationToken token = default
    )
    {
        if (mimeType != Const.MatroskaMimeType && mimeType != Const.Mp4MimeType)
        {
            throw new ArgumentException(
                $"Only '{Const.MatroskaMimeType}' and '{Const.Mp4MimeType}' video container " +
                $"formats are supported."
            );
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
                token
            ))
        {
            throw new InvalidOperationException($"Failed to store shard '{shardId}'.");
        }

        if (!storageService.TryGetFilePath(
                ShardKind.Video,
                shardId,
                Const.OriginalShardVariant,
                out var shardFilePath
            ))
        {
            throw new ArgumentException("The shard stream could not be opened just after being saved.");
        }

        var mediaInfo = await mediaService.GetInfo(shardFilePath, token);

        var created = new VideoShardCreated(
            ShardId: shardId.ToString(),
            CreationMethod: CreationMethod.Api,
            ArtifactId: artifactId.ToString(),
            OriginalVariantInfo: mediaInfo
        );
        db.Events.KafeStartStream<VideoShardInfo>(created.ShardId, created);
        await db.SaveChangesAsync(token);

        return created.ShardId;
    }

    private async Task<Hrib?> CreateImage(
        Hrib artifactId,
        Stream imageStream,
        Hrib? shardId = null,
        CancellationToken token = default
    )
    {
        var imageInfo = await imageService.GetInfo(imageStream, token);

        imageStream.Seek(0, SeekOrigin.Begin);

        shardId ??= Hrib.Create();

        var created = new ImageShardCreated(
            ShardId: shardId.ToString(),
            CreationMethod: CreationMethod.Api,
            ArtifactId: artifactId.ToString(),
            OriginalVariantInfo: imageInfo
        );

        if (!await storageService.TryStoreShard(
                ShardKind.Image,
                created.ShardId,
                imageStream,
                Const.OriginalShardVariant,
                imageInfo.FileExtension,
                token
            ))
        {
            throw new InvalidOperationException("The image could not be stored.");
        }

        db.Events.KafeStartStream<VideoShardInfo>(created.ShardId, created);

        await db.SaveChangesAsync(token);
        return created.ShardId;
    }

    private async Task<Hrib?> CreateSubtitles(
        Hrib artifactId,
        Stream subtitlesStream,
        Hrib? shardId = null,
        CancellationToken token = default
    )
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
            IsCorrupted: mediaInfo.IsCorrupted
        );

        shardId ??= Hrib.Create();

        var created = new SubtitlesShardCreated(
            ShardId: shardId.ToString(),
            CreationMethod: CreationMethod.Api,
            ArtifactId: artifactId.ToString(),
            OriginalVariantInfo: info
        );

        if (!await storageService.TryStoreShard(
                ShardKind.Subtitles,
                shardId,
                subtitlesStream,
                Const.OriginalShardVariant,
                info.FileExtension,
                token
            ))
        {
            throw new InvalidOperationException("The subtitles could not be stored.");
        }

        db.Events.KafeStartStream<SubtitlesShardInfo>(created.ShardId, created);

        await db.SaveChangesAsync(token);
        return created.ShardId;
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
        var shardKind = await GetShardKind(id, token);
        if (!storageService.TryOpenShardStream(shardKind, id, variant, out var stream, out _))
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

    public async Task<VariantInfo?> GetShardVariantMediaType(
        Hrib id,
        string? variant,
        CancellationToken token = default
    )
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
                    MimeType: media.MimeType
                )
                : null,
            ShardKind.Image => ((ImageShardInfo)shard).Variants.TryGetValue(variant, out var image)
                ? new VariantInfo(
                    ShardId: shard.Id,
                    Variant: variant,
                    FileExtension: image.FileExtension,
                    MimeType: image.MimeType
                )
                : null,
            ShardKind.Subtitles => ((SubtitlesShardInfo)shard).Variants.TryGetValue(variant, out var image)
                ? new VariantInfo(
                    ShardId: shard.Id,
                    Variant: variant,
                    FileExtension: image.FileExtension,
                    MimeType: image.MimeType
                )
                : null,
            ShardKind.Blend => ((BlendShardInfo)shard).Variants.TryGetValue(variant, out var media)
                ? new VariantInfo(
                    ShardId: shard.Id,
                    Variant: variant,
                    FileExtension: media.FileExtension,
                    MimeType: media.MimeType
                )
                : null,
            _ => throw new NotImplementedException()
        };
        ;
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
