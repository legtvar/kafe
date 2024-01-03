using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using Kafe.Common;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Capabilities;
using Kafe.Media;

namespace Kafe.Api.Transfer;

public static class TransferMaps
{
    public static readonly ProjectBlueprintDto TemporaryProjectBlueprintMockup
        = new(
            RequiredReviewers: ImmutableArray.Create(
                Const.TechReviewer,
                Const.VisualReviewer,
                Const.DramaturgyReviewer
            ),
            Name: LocalizedString.Create(
                (Const.InvariantCulture, "Film registration to FFFI MU 2023"),
                (Const.CzechCulture, "Přihlášení filmu na 23. FFFI MU")
            ),
            Description: null,
            ArtifactBlueprints: new Dictionary<string, ProjectArtifactBlueprintDto>
            {
                [Const.FilmBlueprintSlot] = new ProjectArtifactBlueprintDto(
                    Name: LocalizedString.Create(
                        (Const.InvariantCulture, "Film"),
                        (Const.CzechCulture, "Film")
                    ),
                    Description: null,
                    Arity: ArgumentArity.ExactlyOne,
                    ShardBlueprints: new Dictionary<ShardKind, ProjectArtifactShardBlueprintDto>
                    {
                        [ShardKind.Video] = new ProjectArtifactShardBlueprintDto(
                            Name: LocalizedString.Create(
                                (Const.InvariantCulture, "Film file"),
                                (Const.CzechCulture, "Soubor s filmem")
                            ),
                            Description: null,
                            Arity: ArgumentArity.ExactlyOne),
                        [ShardKind.Subtitles] = new ProjectArtifactShardBlueprintDto(
                            Name: LocalizedString.Create(
                                (Const.InvariantCulture, "English subtitles"),
                                (Const.CzechCulture, "Anglické titulky")
                            ),
                            Description: null,
                            Arity: ArgumentArity.ExactlyOne)
                    }
                    .ToImmutableDictionary()
                ),
                [Const.VideoAnnotationBlueprintSlot] = new ProjectArtifactBlueprintDto(
                    Name: LocalizedString.Create(
                        (Const.InvariantCulture, "Video-annotation"),
                        (Const.CzechCulture, "Videoanotace")
                    ),
                    Description: null,
                    Arity: ArgumentArity.ZeroOrOne,
                    ShardBlueprints: new Dictionary<ShardKind, ProjectArtifactShardBlueprintDto>
                    {
                        [ShardKind.Video] = new ProjectArtifactShardBlueprintDto(
                            Name: LocalizedString.Create(
                                (Const.InvariantCulture, "Video-annotation file"),
                                (Const.CzechCulture, "Soubor s videoanotací")
                            ),
                            Description: null,
                            Arity: ArgumentArity.ExactlyOne),
                        [ShardKind.Subtitles] = new ProjectArtifactShardBlueprintDto(
                            Name: LocalizedString.Create(
                                (Const.InvariantCulture, "English subtitles"),
                                (Const.CzechCulture, "Anglické titulky")
                            ),
                            Description: null,
                            Arity: ArgumentArity.ExactlyOne)
                    }
                    .ToImmutableDictionary()
                ),
                [Const.CoverPhotoBlueprintSlot] = new ProjectArtifactBlueprintDto(
                    Name: LocalizedString.Create(
                        (Const.InvariantCulture, "Cover photo"),
                        (Const.CzechCulture, "Titulní fotografie")
                    ),
                    Description: null,
                    Arity: new ArgumentArity(Const.CoverPhotoMinCount, Const.CoverPhotoMaxCount),
                    ShardBlueprints: new Dictionary<ShardKind, ProjectArtifactShardBlueprintDto>
                    {
                        [ShardKind.Image] = new ProjectArtifactShardBlueprintDto(
                            Name: LocalizedString.Create(
                                (Const.InvariantCulture, "Cover photo file"),
                                (Const.CzechCulture, "Soubor s titulní fotografií")
                            ),
                            Description: null,
                            Arity: ArgumentArity.ExactlyOne)
                    }
                    .ToImmutableDictionary()
                )
            }
            .ToImmutableDictionary()
        );


    public static ProjectListDto ToProjectListDto(ProjectInfo data, Permission userPermission = Permission.None)
    {
        return new ProjectListDto(
            Id: data.Id,
            ProjectGroupId: data.ProjectGroupId,
            Name: (LocalizedString)data.Name,
            Description: data.Description,
            GlobalPermissions: ToPermissionArray(data.GlobalPermissions),
            UserPermissions: ToPermissionArray(data.GlobalPermissions | userPermission),
            ReleasedOn: data.ReleasedOn);
    }

    public static ProjectDetailDto ToProjectDetailDto(ProjectInfo data, Permission userPermission = Permission.None)
    {
        return new ProjectDetailDto(
            Id: data.Id,
            ProjectGroupId: data.ProjectGroupId,
            ProjectGroupName: null,
            Genre: data.Genre,
            Name: data.Name,
            Description: data.Description,
            GlobalPermissions: ToPermissionArray(data.GlobalPermissions),
            UserPermissions: ToPermissionArray(data.GlobalPermissions | userPermission),
            ReleasedOn: data.ReleasedOn,
            Crew: ImmutableArray<ProjectAuthorDto>.Empty,
            Cast: ImmutableArray<ProjectAuthorDto>.Empty,
            Artifacts: ImmutableArray<ProjectArtifactDto>.Empty,
            Reviews: data.Reviews.IsDefaultOrEmpty
                ? ImmutableArray<ProjectReviewDto>.Empty
                : data.Reviews.Select(ToProjectReviewDto).ToImmutableArray(),
            Blueprint: TemporaryProjectBlueprintMockup
        );
    }

    public static ProjectReviewDto ToProjectReviewDto(ProjectReviewInfo review)
    {
        return new ProjectReviewDto(
            Kind: review.Kind,
            ReviewerRole: review.ReviewerRole,
            Comment: review.Comment,
            AddedOn: review.AddedOn
        );
    }

    public static AuthorListDto ToAuthorListDto(AuthorInfo data)
    {
        return new AuthorListDto(
            Id: data.Id,
            Name: data.Name,
            GlobalPermissions: data.GlobalPermissions);
    }

    public static AuthorDetailDto ToAuthorDetailDto(AuthorInfo data)
    {
        return new AuthorDetailDto(
            Id: data.Id,
            Name: data.Name,
            GlobalPermissions: data.GlobalPermissions,
            Bio: data.Bio,
            Uco: data.Uco,
            Email: data.Email,
            Phone: data.Phone);
    }

    public static PlaylistListDto ToPlaylistListDto(PlaylistInfo data)
    {
        return new PlaylistListDto(
            Id: data.Id,
            Name: data.Name,
            Description: data.Description,
            GlobalPermissions: ToPermissionArray(data.GlobalPermissions));
    }

    public static PlaylistDetailDto ToPlaylistDetailDto(PlaylistInfo data)
    {
        return new PlaylistDetailDto(
            Id: data.Id,
            Name: data.Name,
            Description: data.Description,
            GlobalPermissions: ToPermissionArray(data.GlobalPermissions),
            Entries: ImmutableArray<PlaylistEntryDto>.Empty);
    }

    public static ProjectGroupListDto ToProjectGroupListDto(ProjectGroupInfo data)
    {
        return new ProjectGroupListDto(
            Id: data.Id,
            Name: data.Name,
            Description: data.Description,
            Deadline: data.Deadline,
            IsOpen: data.IsOpen);
    }

    public static ProjectGroupDetailDto ToProjectGroupDetailDto(ProjectGroupInfo data)
    {
        return new ProjectGroupDetailDto(
            Id: data.Id,
            Name: data.Name,
            Description: data.Description,
            Deadline: data.Deadline,
            IsOpen: data.IsOpen,
            Projects: ImmutableArray<ProjectListDto>.Empty);
    }

    public static ArtifactDetailDto ToArtifactDetailDto(ArtifactDetail data)
    {
        return new ArtifactDetailDto(
            Id: data.Id,
            Name: data.Name,
            Shards: data.Shards.Select(ToShardListDto).ToImmutableArray(),
            ContainingProjectIds: data.ContainingProjectIds.Select(i => (Hrib)i).ToImmutableArray(),
            AddedOn: data.AddedOn
        );
    }

    public static ProjectArtifactDto ToProjectArtifactDto(ArtifactDetail data)
    {
        return new ProjectArtifactDto(
            Id: data.Id,
            Name: data.Name,
            AddedOn: data.AddedOn,
            BlueprintSlot: null,
            Shards: data.Shards.Select(ToShardListDto).ToImmutableArray()
        );
    }

    public static ShardListDto ToShardListDto(ArtifactShardInfo data)
    {
        return new ShardListDto(
            Id: data.ShardId,
            Kind: data.Kind,
            Variants: data.Variants.ToImmutableArray());
    }

    public static VideoShardDetailDto ToVideoShardDetailDto(VideoShardInfo data)
    {
        return new VideoShardDetailDto(
            Id: data.Id,
            Kind: data.Kind,
            ArtifactId: data.ArtifactId,
            Variants: data.Variants.ToImmutableDictionary(v => v.Key, v => ToMediaInfoDto(v.Value)));
    }

    public static MediaDto ToMediaInfoDto(MediaInfo data)
    {
        return new MediaDto(
            FileExtension: data.FileExtension,
            MimeType: data.MimeType,
            FileLength: data.FileLength,
            Duration: data.Duration,
            VideoStreams: data.VideoStreams.Select(ToVideoStreamDto).ToImmutableArray(),
            AudioStreams: data.AudioStreams.Select(ToAudioStreamDto).ToImmutableArray(),
            SubtitleStreams: data.SubtitleStreams.Select(ToSubtitleStreamDto).ToImmutableArray(),
            IsCorrupted: data.IsCorrupted,
            Error: data.Error);
    }

    public static VideoStreamDto ToVideoStreamDto(VideoStreamInfo data)
    {
        return new VideoStreamDto(
            Codec: data.Codec,
            Bitrate: data.Bitrate,
            Width: data.Width,
            Height: data.Height,
            Framerate: data.Framerate);
    }

    public static AudioStreamDto ToAudioStreamDto(AudioStreamInfo data)
    {
        return new AudioStreamDto(
            Codec: data.Codec,
            Bitrate: data.Bitrate,
            Channels: data.Channels,
            SampleRate: data.SampleRate);
    }

    public static SubtitleStreamDto ToSubtitleStreamDto(SubtitleStreamInfo data)
    {
        return new SubtitleStreamDto(
            Codec: data.Codec,
            Bitrate: data.Bitrate);
    }

    public static ImageShardDetailDto ToImageShardDetailDto(ImageShardInfo data)
    {
        return new ImageShardDetailDto(
            Id: data.Id,
            Kind: data.Kind,
            ArtifactId: data.ArtifactId,
            Variants: data.Variants.ToImmutableDictionary(v => v.Key, v => ToImageDto(v.Value)));
    }

    public static ImageDto ToImageDto(ImageInfo data)
    {
        return new ImageDto(
            FileExtension: data.FileExtension,
            MimeType: data.MimeType,
            Width: data.Width,
            Height: data.Height,
            IsCorrupted: data.IsCorrupted);
    }

    public static ShardDetailBaseDto ToShardDetailDto(IShardEntity data)
    {
        return data switch
        {
            VideoShardInfo v => ToVideoShardDetailDto(v),
            ImageShardInfo i => ToImageShardDetailDto(i),
            SubtitlesShardInfo s => ToSubtitlesShardDetailDto(s),
            _ => throw new NotSupportedException($"Shards of '{data.GetType()}' are not supported.")
        };
    }

    public static SubtitlesShardDetailDto ToSubtitlesShardDetailDto(SubtitlesShardInfo data)
    {
        return new SubtitlesShardDetailDto(
            Id: data.Id,
            Kind: data.Kind,
            ArtifactId: data.ArtifactId,
            Variants: data.Variants.ToImmutableDictionary(p => p.Key, p => ToSubtitlesDto(p.Value)));
    }

    public static SubtitlesDto ToSubtitlesDto(SubtitlesInfo data)
    {
        return new SubtitlesDto(
            FileExtension: data.FileExtension,
            MimeType: data.MimeType,
            Language: data.Language,
            Codec: data.Codec,
            Bitrate: data.Bitrate);
    }

    public static TemporaryAccountInfoDto ToTemporaryAccountInfoDto(AccountInfo data)
    {
        return new TemporaryAccountInfoDto(
            Id: data.Id,
            EmailAddress: data.EmailAddress,
            PreferredCulture: data.PreferredCulture);
    }

    public static AccountDetailDto ToAccountDetailDto(
        AccountInfo data)
    {
        return new AccountDetailDto(
            Id: data.Id,
            Name: data.Name,
            Uco: data.Uco,
            EmailAddress: data.EmailAddress,
            PreferredCulture: data.PreferredCulture,
            Permissions: data.Permissions?.ToImmutableDictionary(p => (Hrib)p.Key, p => ToPermissionArray(p.Value))
                ?? ImmutableDictionary<Hrib, ImmutableArray<Permission>>.Empty
        );
    }

    public static AccountListDto ToAccountListDto(
        AccountInfo data)
    {
        return new AccountListDto(
            Id: data.Id,
            EmailAddress: data.EmailAddress,
            PreferredCulture: data.PreferredCulture,
            Permissions: data?.Permissions?.ToImmutableDictionary(p => (Hrib)p.Key, p => ToPermissionArray(p.Value))
                ?? ImmutableDictionary<Hrib, ImmutableArray<Permission>>.Empty
        );
    }

    public static EntityPermissionsDetailDto ToEntityPermissionsDetailDto(
        Hrib id,
        string? entityType,
        Permission? globalPermissions,
        Permission? userPermissions,
        IEnumerable<AccountInfo> accounts
    )
    {
        return new EntityPermissionsDetailDto(
            Id: id.ToString(),
            EntityType: entityType,
            GlobalPermissions: globalPermissions is null ? null : ToPermissionArray(globalPermissions.Value),
            UserPermissions: userPermissions is null ? null : ToPermissionArray(userPermissions.Value),
            AccountPermissions: accounts.Select(a => new EntityPermissionsAccountListDto(
                Id: a.Id,
                EmailAddress: a.EmailAddress,
                Permissions: ToPermissionArray(a.Permissions?.GetValueOrDefault(id.ToString()) ?? Permission.None)
            )).ToImmutableArray()
        );
    }
    
    public static Permission FromPermissionArray(ImmutableArray<Permission>? array)
    {
        if (array is null || array.Value.IsDefaultOrEmpty)
        {
            return Permission.None;
        }

        return array.Value.Aggregate(Permission.None, (lhs, rhs) => lhs | rhs);
    }

    public static ImmutableArray<Permission> ToPermissionArray(Permission value)
    {
        if (value == Permission.None)
        {
            return ImmutableArray<Permission>.Empty;
        }

        return Enum.GetValues<Permission>()
            .Where(v => v != Permission.None && v != Permission.All && (value & v) == v)
            .ToImmutableArray();
    }
}
