using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Media;

namespace Kafe.Api.Transfer;

public static class TransferMaps
{
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
                        ))
                        .ToImmutableDictionary()
                ))
                .ToImmutableDictionary()
        );
    }

    public static readonly ProjectBlueprintDto TemporaryProjectBlueprintMockup = 
        ToProjectBlueprintDto(ProjectBlueprint.TemporaryProjectBlueprint);
        
    public static ProjectBlueprintDto TemporaryMateProjectBlueprintMockup = 
        ToProjectBlueprintDto(ProjectBlueprint.TemporaryMateProjectBlueprint);

    public static ProjectListDto ToProjectListDto(ProjectInfo data, Permission userPermission = Permission.None)
    {
        return new ProjectListDto(
            Id: data.Id,
            ProjectGroupId: data.ProjectGroupId,
            Name: (LocalizedString)data.Name,
            Description: data.Description,
            GlobalPermissions: ToPermissionArray(data.GlobalPermissions),
            UserPermissions: ToPermissionArray(data.GlobalPermissions | userPermission),
            ReleasedOn: data.ReleasedOn,
            IsLocked: data.IsLocked,
            LatestReviewKind: data.Reviews != null && !data.Reviews.IsDefaultOrEmpty
            ? data.Reviews.OrderByDescending(r => r.AddedOn).First().Kind
            : ReviewKind.NotReviewed,
            OwnerId: data.OwnerId);
    }

    public static ProjectDetailDto ToProjectDetailDto(ProjectInfo data, Permission userPermission = Permission.None)
    {
        return new ProjectDetailDto(
            Id: data.Id,
            ProjectGroupId: data.ProjectGroupId,
            ProjectGroupName: null,
            ValidationSettings: ProjectValidationSettings.Default,
            Genre: data.Genre,
            Name: data.Name,
            Description: data.Description,
            GlobalPermissions: ToPermissionArray(data.GlobalPermissions),
            UserPermissions: ToPermissionArray(data.GlobalPermissions | userPermission),
            ReleasedOn: data.ReleasedOn,
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
            OrganizationId: data.OrganizationId,
            Name: data.Name,
            Description: data.Description,
            GlobalPermissions: ToPermissionArray(data.GlobalPermissions));
    }

    public static PlaylistDetailDto ToPlaylistDetailDto(PlaylistInfo data)
    {
        return new PlaylistDetailDto(
            Id: data.Id,
            OrganizationId: data.OrganizationId,
            Name: data.Name,
            Description: data.Description,
            GlobalPermissions: ToPermissionArray(data.GlobalPermissions),
            Entries: []);
    }

    public static ProjectGroupListDto ToProjectGroupListDto(ProjectGroupInfo data)
    {
        return new ProjectGroupListDto(
            Id: data.Id,
            OrganizationId: data.OrganizationId,
            Name: data.Name,
            Description: data.Description,
            Deadline: data.Deadline,
            IsOpen: data.IsOpen);
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

    public static ArtifactDetailDto ToArtifactDetailDto(ArtifactDetail data)
    {
        return new ArtifactDetailDto(
            Id: data.Id,
            Name: data.Name,
            Shards: [..data.Shards.Select(ToShardListDto)],
            ContainingProjectIds: [..data.ContainingProjectIds.Select(i => (Hrib)i)],
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
            VideoStreams: [.. data.VideoStreams.Select(ToVideoStreamDto)],
            AudioStreams: [.. data.AudioStreams.Select(ToAudioStreamDto)],
            SubtitleStreams: [.. data.SubtitleStreams.Select(ToSubtitleStreamDto)],
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
            BlendShardInfo b => ToBlendShardDetailDto(b),
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

    public static BlendShardDetailDto ToBlendShardDetailDto(BlendShardInfo data)
    {
        return new BlendShardDetailDto(
            Id: data.Id,
            FileName: data.FileName,
            Kind: data.Kind,
            ArtifactId: data.ArtifactId,
            Variants: data.Variants.ToImmutableDictionary(p => p.Key, p => ToBlendDto(p.Value)));
    }

    public static BlendDto ToBlendDto(BlendInfo data)
    {
        return new BlendDto(
            FileExtension: data.FileExtension,
            MimeType: data.MimeType,
            Tests: data.Tests?.Select(ToBlendTestResponseDto).ToImmutableArray(),
            Error: data.Error);
    }

    public static PigeonsTestInfoDto ToBlendTestResponseDto(PigeonsTestInfo data)
    {
        return new PigeonsTestInfoDto(
            Label: data.Label,
            State: data.State,
            Datablock: data.Datablock,
            Message: data.Message,
            Traceback: data.Traceback);
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
        IEnumerable<InviteInfo> invites,
        IEnumerable<AccountInfo> accounts
    )
    {
        return new EntityPermissionsDetailDto(
            Id: id.ToString(),
            EntityType: entityType,
            GlobalPermissions: globalPermissions is null ? null : ToPermissionArray(globalPermissions.Value),
            UserPermissions: userPermissions is null ? null : ToPermissionArray(userPermissions.Value),
            AccountPermissions: [
                ..accounts.Select(a => new EntityPermissionsAccountListDto(
                    Id: a.Id,
                    EmailAddress: a.EmailAddress,
                    Name: a.Name,
                    Permissions: ToPermissionArray(a.Permissions?.GetValueOrDefault(id.ToString()) ?? Permission.None)
                )).Concat(invites.Select(i => new EntityPermissionsAccountListDto(
                    Id: null,
                    EmailAddress: i.EmailAddress,
                    Name: null,
                    Permissions: ToPermissionArray(i.Permissions?.GetValueOrDefault(id.ToString())?.Permission
                        ?? Permission.None)
                    )))
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

        return Enum.GetValues<Permission>()
            .Where(v => v != Permission.None
                && v != Permission.All
                && v != Permission.Inheritable
                && v != Permission.Publishable
                && (value & v) == v)
            .ToImmutableArray();
    }

    public static OrganizationDetailDto ToOrganizationDetailDto(OrganizationInfo data)
    {
        return new(
            Id: data.Id,
            Name: data.Name,
            CreatedOn: data.CreatedOn
        );
    }

    public static OrganizationListDto ToOrganizationListDto(OrganizationInfo data)
    {
        return new(
            Id: data.Id,
            Name: data.Name
        );
    }

    public static RoleDetailDto ToRoleDetailDto(RoleInfo data)
    {
        return new(
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
        return new(
            Id: data.Id,
            OrganizationId: data.OrganizationId,
            Name: data.Name
        );
    }
}
