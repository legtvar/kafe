using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Legacy.Corrections;
using Kafe.Mate;
using Kafe.Media;

namespace Kafe.Api.Transfer;

public static class TransferMaps
{
    [Obsolete("Use the new artifact abstraction instead.")]
    public static ProjectBlueprintDto ToProjectBlueprintDto(ProjectBlueprint projectBlueprint)
    {
        return new ProjectBlueprintDto(
            RequiredReviewers: projectBlueprint.RequiredReviewers,
            ArtifactBlueprints: projectBlueprint.ArtifactBlueprints.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new ProjectArtifactBlueprintDto(
                        Name: kvp.Value.Name,
                        Description: kvp.Value.Description,
                        Arity: kvp.Value.Arity,
                        ShardBlueprints: kvp.Value.ShardBlueprints.ToDictionary(
                                skvp => skvp.Key,
                                skvp => new ProjectArtifactShardBlueprintDto(
                                    Name: skvp.Value.Name,
                                    Description: skvp.Value.Description,
                                    Arity: skvp.Value.Arity
                                )
                            )
                            .ToImmutableDictionary()
                    )
                )
                .ToImmutableDictionary()
        );
    }


    [Obsolete("Use the new artifact abstraction instead.")]
    public static ProjectBlueprintDto GetProjectBlueprintDtoByOrgId(string id)
    {
        switch (id)
        {
            case "mate-fimuni":
                return TemporaryMateProjectBlueprintMockup;
            case "lemmafimuni":
                return TemporaryLemmaProjectBlueprintMockup;
            default:
                return TemporaryProjectBlueprintMockup;
        }
    }

    [Obsolete("Use the new artifact abstraction instead.")]
    public static readonly ProjectBlueprintDto TemporaryProjectBlueprintMockup
        = ToProjectBlueprintDto(ProjectBlueprint.TemporaryProjectBlueprint);

    [Obsolete("Use the new artifact abstraction instead.")]
    public static ProjectBlueprintDto TemporaryLemmaProjectBlueprintMockup
        = ToProjectBlueprintDto(ProjectBlueprint.TemporaryLemmaProjectBlueprint);

    [Obsolete("Use the new artifact abstraction instead.")]
    public static readonly ProjectBlueprintDto TemporaryMateProjectBlueprintMockup
        = ToProjectBlueprintDto(ProjectBlueprint.TemporaryMateProjectBlueprint);


    public static ProjectListDto ToProjectListDto(
        ProjectInfo data,
        ArtifactInfo? artifact,
        Permission userPermission = Permission.None
    )
    {
        return new ProjectListDto(
            Id: data.Id,
            ProjectGroupId: data.ProjectGroupId,
            Name: artifact?.Name ?? Const.UnnamedProjectName,
            Description: artifact?.GetProperty<LocalizedString>(LegacyBlueprintsCorrection.DescriptionProp),
            GlobalPermissions: ToPermissionArray(data.GlobalPermissions),
            UserPermissions: ToPermissionArray(data.GlobalPermissions | userPermission),
            ReleasedOn: artifact?.GetProperty<DateTimeOffset>(LegacyBlueprintsCorrection.ReleasedOnProp),
            IsLocked: data.IsLocked,
            LatestReviewKind: data.Reviews != null && !data.Reviews.IsDefaultOrEmpty
                ? data.Reviews.OrderByDescending(r => r.AddedOn).First().Kind
                : ReviewKind.NotReviewed,
            OwnerId: data.OwnerId
        );
    }

    [Obsolete("Use the new artifact abstraction instead.")]
    public static ProjectDetailDto ToProjectDetailDto(
        ProjectInfo data,
        ArtifactInfo? artifact,
        Permission userPermission = Permission.None
    )
    {
        return new ProjectDetailDto(
            Id: data.Id,
            ProjectGroupId: data.ProjectGroupId,
            ProjectGroupName: null,
            ValidationSettings: ProjectValidationSettings.Default,
            Genre: artifact?.GetProperty<LocalizedString>(LegacyBlueprintsCorrection.GenreProp),
            Name: artifact?.Name ?? Const.UnnamedProjectName,
            Description: artifact?.GetProperty<LocalizedString>(LegacyBlueprintsCorrection.DescriptionProp),
            GlobalPermissions: ToPermissionArray(data.GlobalPermissions),
            UserPermissions: ToPermissionArray(data.GlobalPermissions | userPermission),
            ReleasedOn: artifact?.GetProperty<DateTimeOffset>(LegacyBlueprintsCorrection.ReleasedOnProp),
            AiUsageDeclaration: artifact?.GetProperty<string>(LegacyBlueprintsCorrection.AiUsageDeclarationProp),
            HearAboutUs: artifact?.GetProperty<string>(LegacyBlueprintsCorrection.HearAboutUsProp),
            Crew: [],
            Cast: [],
            OwnerId: data.OwnerId,
            Artifacts: [],
            Reviews: data.Reviews.IsDefaultOrEmpty
                ? []
                : data.Reviews.Select(ToProjectReviewDto).ToImmutableArray(),
            Blueprint: TemporaryProjectBlueprintMockup,
            IsLocked: data.IsLocked
        );
    }

    public static ProjectReviewDto ToProjectReviewDto(ProjectReviewInfo review)
    {
        return new ProjectReviewDto(
            ReviewerId: review.ReviewerId,
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
            GlobalPermissions: data.GlobalPermissions
        );
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
            Phone: data.Phone
        );
    }

    public static PlaylistListDto ToPlaylistListDto(PlaylistInfo data)
    {
        return new PlaylistListDto(
            Id: data.Id,
            OrganizationId: data.OrganizationId,
            Name: data.Name,
            Description: data.Description,
            GlobalPermissions: ToPermissionArray(data.GlobalPermissions)
        );
    }

    public static PlaylistDetailDto ToPlaylistDetailDto(PlaylistInfo data)
    {
        return new PlaylistDetailDto(
            Id: data.Id,
            OrganizationId: data.OrganizationId,
            Name: data.Name,
            Description: data.Description,
            GlobalPermissions: ToPermissionArray(data.GlobalPermissions),
            Entries: []
        );
    }

    public static ProjectGroupListDto ToProjectGroupListDto(ProjectGroupInfo data)
    {
        return new ProjectGroupListDto(
            Id: data.Id,
            OrganizationId: data.OrganizationId,
            Name: data.Name,
            Description: data.Description,
            Deadline: data.Deadline,
            IsOpen: data.IsOpen
        );
    }

    public static ProjectGroupDetailDto ToProjectGroupDetailDto(ProjectGroupInfo data)
    {
        return new ProjectGroupDetailDto(
            Id: data.Id,
            OrganizationId: data.OrganizationId,
            Name: data.Name,
            Description: data.Description,
            Deadline: data.Deadline,
            IsOpen: data.IsOpen,
            Projects: [],
            ValidationSettings: ProjectValidationSettings.Merge(
                data.ValidationSettings,
                ProjectValidationSettings.Default
            )
        );
    }

    public static ArtifactDetailDto ToArtifactDetailDto(ArtifactInfo data)
    {
        return new ArtifactDetailDto(
            Id: data.Id,
            Name: data.Name,
            Shards: [],
            ContainingProjectIds: [],
            AddedOn: data.AddedOn
        );
    }

    public static ProjectArtifactDto ToProjectArtifactDto(ArtifactInfo data)
    {
        return new ProjectArtifactDto(
            Id: data.Id,
            Name: data.Name,
            AddedOn: data.AddedOn,
            BlueprintSlot: null,
            Shards: []
        );
    }

    [Obsolete("Use the new artifact abstraction instead.")]
    public static ShardListDto ToShardListDto(ShardInfo data)
    {
        return new ShardListDto(
            Id: data.Id,
            Kind: ShardCompat.ToShardKind(data.Payload.Value.GetType()),
            Variants:
            [
                Const.OriginalShardVariant,
                ..data.Links.Select(l => l.Payload.Value)
                    .OfType<VariantShardLink>()
                    .Select(v => v.Preset)
                    .OfType<string>()
            ]
        );
    }

    public static MediaDto ToMediaInfoDto(MediaInfo data)
    {
        return new MediaDto(
            FileExtension: data.FileExtension,
            MimeType: data.MimeType,
            FileLength: data.FileLength,
            Duration: data.Duration,
            VideoStreams: [.. data.VideoStreams.Select(ToVideoStreamDto)],
            AudioStreams: [.. data.AudioStreams.Select(ToAudioStreamDto)],
            SubtitleStreams: [.. data.SubtitleStreams.Select(ToSubtitleStreamDto)],
            IsCorrupted: data.IsCorrupted,
            Error: data.Error
        );
    }

    public static VideoStreamDto ToVideoStreamDto(VideoStreamInfo data)
    {
        return new VideoStreamDto(
            Codec: data.Codec,
            Bitrate: data.Bitrate,
            Width: data.Width,
            Height: data.Height,
            Framerate: data.Framerate
        );
    }

    public static AudioStreamDto ToAudioStreamDto(AudioStreamInfo data)
    {
        return new AudioStreamDto(
            Codec: data.Codec,
            Bitrate: data.Bitrate,
            Channels: data.Channels,
            SampleRate: data.SampleRate
        );
    }

    public static SubtitleStreamDto ToSubtitleStreamDto(SubtitleStreamInfo data)
    {
        return new SubtitleStreamDto(
            Codec: data.Codec,
            Bitrate: data.Bitrate
        );
    }

    public static ImageDto ToImageDto(ImageInfo data)
    {
        return new ImageDto(
            FileExtension: data.FileExtension,
            MimeType: data.MimeType,
            Width: data.Width,
            Height: data.Height,
            IsCorrupted: data.IsCorrupted
        );
    }

    [Obsolete("Use the new artifact abstraction instead.")]
    public static ShardDetailBaseDto ToShardDetailDto(ShardInfo data)
    {
        var kind = ShardCompat.ToShardKind(data.Payload.Value.GetType());
        return kind switch
        {
            ShardKind.Video => new VideoShardDetailDto(
                Id: data.Id,
                Kind: ShardKind.Video,
                ArtifactId: Hrib.Empty,
                Variants: ImmutableDictionary.CreateRange(
                    [
                        new KeyValuePair<string, MediaDto>(
                            Const.OriginalShardVariant,
                            ToMediaInfoDto((MediaInfo)data.Payload.Value)
                        )
                    ]
                )
            ),
            ShardKind.Image => new ImageShardDetailDto(
                Id: data.Id,
                Kind: ShardKind.Image,
                ArtifactId: Hrib.Empty,
                Variants: ImmutableDictionary.CreateRange(
                    [
                        new KeyValuePair<string, ImageDto>(
                            Const.OriginalShardVariant,
                            ToImageDto((ImageInfo)data.Payload.Value)
                        )
                    ]
                )
            ),
            ShardKind.Subtitles => new SubtitlesShardDetailDto(
                Id: data.Id,
                Kind: ShardKind.Subtitles,
                ArtifactId: Hrib.Empty,
                Variants: ImmutableDictionary.CreateRange(
                    [
                        new KeyValuePair<string, SubtitlesDto>(
                            Const.OriginalShardVariant,
                            ToSubtitlesDto((SubtitlesInfo)data.Payload.Value)
                        )
                    ]
                )
            ),
            ShardKind.Blend => new BlendShardDetailDto(
                Id: data.Id,
                Kind: ShardKind.Blend,
                ArtifactId: Hrib.Empty,
                FileName: data.UploadFilename,
                Variants: ImmutableDictionary.CreateRange(
                    [
                        new KeyValuePair<string, BlendDto>(
                            Const.OriginalShardVariant,
                            ToBlendDto((BlendInfo)data.Payload.Value)
                        )
                    ]
                )
            ),
            _ => throw new NotSupportedException($"Shards of '{data.GetType()}' are not supported.")
        };
    }

    public static SubtitlesDto ToSubtitlesDto(SubtitlesInfo data)
    {
        return new SubtitlesDto(
            FileExtension: data.FileExtension,
            MimeType: data.MimeType,
            Language: data.Language,
            Codec: data.Codec,
            Bitrate: data.Bitrate
        );
    }

    public static BlendDto ToBlendDto(BlendInfo data)
    {
        return new BlendDto(
            FileExtension: data.FileExtension,
            MimeType: data.MimeType,
            Tests: data.Tests?.Select(ToBlendTestResponseDto).ToImmutableArray(),
            Error: data.Error
        );
    }

    public static PigeonsTestInfoDto ToBlendTestResponseDto(PigeonsTestInfo data)
    {
        return new PigeonsTestInfoDto(
            Label: data.Label,
            State: data.State,
            Datablock: data.Datablock,
            Message: data.Message,
            Traceback: data.Traceback
        );
    }

    public static AccountDetailDto ToAccountDetailDto(
        AccountInfo data
    )
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
        AccountInfo data
    )
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
        IEnumerable<InviteInfo> invites,
        IEnumerable<AccountInfo> accounts
    )
    {
        return new EntityPermissionsDetailDto(
            Id: id.ToString(),
            EntityType: entityType,
            GlobalPermissions: globalPermissions is null ? null : ToPermissionArray(globalPermissions.Value),
            UserPermissions: userPermissions is null ? null : ToPermissionArray(userPermissions.Value),
            AccountPermissions:
            [
                ..accounts.Select(a => new EntityPermissionsAccountListDto(
                            Id: a.Id,
                            EmailAddress: a.EmailAddress,
                            Name: a.Name,
                            Permissions: ToPermissionArray(
                                a.Permissions?.GetValueOrDefault(id.ToString()) ?? Permission.None
                            )
                        )
                    ).Concat(
                        invites.Select(i => new EntityPermissionsAccountListDto(
                                Id: null,
                                EmailAddress: i.EmailAddress,
                                Name: null,
                                Permissions: ToPermissionArray(
                                    i.Permissions?.GetValueOrDefault(id.ToString())?.Permission
                                    ?? Permission.None
                                )
                            )
                        )
                    )
                    .OrderBy(e => e.EmailAddress)
            ]
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
            return [];
        }

        return
        [
            ..Enum.GetValues<Permission>()
                .Where(v => v != Permission.None
                    && v != Permission.All
                    && v != Permission.Inheritable
                    && v != Permission.Publishable
                    && (value & v) == v
                )
        ];
    }

    public static OrganizationDetailDto ToOrganizationDetailDto(OrganizationInfo data)
    {
        return new OrganizationDetailDto(
            Id: data.Id,
            Name: data.Name,
            CreatedOn: data.CreatedOn
        );
    }

    public static OrganizationListDto ToOrganizationListDto(OrganizationInfo data)
    {
        return new OrganizationListDto(
            Id: data.Id,
            Name: data.Name
        );
    }

    public static RoleDetailDto ToRoleDetailDto(RoleInfo data)
    {
        return new RoleDetailDto(
            Id: data.Id,
            OrganizationId: data.OrganizationId,
            Name: data.Name,
            Description: data.Description,
            CreatedOn: data.CreatedOn,
            Permissions: data.Permissions?.ToImmutableDictionary(p => (Hrib)p.Key, p => ToPermissionArray(p.Value))
            ?? ImmutableDictionary<Hrib, ImmutableArray<Permission>>.Empty
        );
    }

    public static RoleListDto ToRoleListDto(RoleInfo data)
    {
        return new RoleListDto(
            Id: data.Id,
            OrganizationId: data.OrganizationId,
            Name: data.Name
        );
    }
}
