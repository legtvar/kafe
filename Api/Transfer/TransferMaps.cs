﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Capabilities;
using Kafe.Media;

namespace Kafe.Api.Transfer;

public static class TransferMaps
{
    private static readonly ProjectBlueprintDto TemporaryProjectBlueprintMockup
        = new(
            Name: LocalizedString.Create(
                (Const.InvariantCulture, "An FFFI MU registration"),
                (Const.CzechCulture, "Přihláška na FFFI MU")
            ),
            Description: null,
            ArtifactBlueprints: ImmutableArray.Create(
                new ProjectArtifactBlueprintDto(
                    Name: LocalizedString.Create(
                        (Const.InvariantCulture, "Film"),
                        (Const.CzechCulture, "Film")
                    ),
                    Description: null,
                    SlotName: "film",
                    Arity: ArgumentArity.ExactlyOne,
                    ShardBlueprints: ImmutableArray.Create(
                        new ProjectArtifactShardBlueprintDto(
                            Name: LocalizedString.Create(
                                (Const.InvariantCulture, "Film file"),
                                (Const.CzechCulture, "Soubor s filmem")
                            ),
                            Description: null,
                            Kind: ShardKind.Video,
                            Arity: ArgumentArity.ExactlyOne),
                        new ProjectArtifactShardBlueprintDto(
                            Name: LocalizedString.Create(
                                (Const.InvariantCulture, "English subtitles"),
                                (Const.CzechCulture, "Anglické titulky")
                            ),
                            Description: null,
                            Kind: ShardKind.Subtitles,
                            Arity: ArgumentArity.ExactlyOne)
                    )
                ),
                new ProjectArtifactBlueprintDto(
                    Name: LocalizedString.Create(
                        (Const.InvariantCulture, "Video-annotation"),
                        (Const.CzechCulture, "Videoanotace")
                    ),
                    Description: null,
                    SlotName: "video-annotation",
                    Arity: ArgumentArity.ZeroOrOne,
                    ShardBlueprints: ImmutableArray.Create(
                        new ProjectArtifactShardBlueprintDto(
                            Name: LocalizedString.Create(
                                (Const.InvariantCulture, "Video-annotation file"),
                                (Const.CzechCulture, "Soubor s videoanotací")
                            ),
                            Description: null,
                            Kind: ShardKind.Video,
                            Arity: ArgumentArity.ExactlyOne),
                        new ProjectArtifactShardBlueprintDto(
                            Name: LocalizedString.Create(
                                (Const.InvariantCulture, "English subtitles"),
                                (Const.CzechCulture, "Anglické titulky")
                            ),
                            Description: null,
                            Kind: ShardKind.Subtitles,
                            Arity: ArgumentArity.ExactlyOne)
                    )
                ),
                new ProjectArtifactBlueprintDto(
                    Name: LocalizedString.Create(
                        (Const.InvariantCulture, "Cover photo"),
                        (Const.CzechCulture, "Titulní fotografie")
                    ),
                    Description: null,
                    SlotName: "cover-photo",
                    Arity: new ArgumentArity(1, 5),
                    ShardBlueprints: ImmutableArray.Create(
                        new ProjectArtifactShardBlueprintDto(
                            Name: LocalizedString.Create(
                                (Const.InvariantCulture, "Cover photo file"),
                                (Const.CzechCulture, "Soubor s titulní fotografií")
                            ),
                            Description: null,
                            Kind: ShardKind.Image,
                            Arity: ArgumentArity.ExactlyOne)
                    )
                )
            )
        );


    public static ProjectListDto ToProjectListDto(ProjectInfo data)
    {
        return new ProjectListDto(
            Id: data.Id,
            ProjectGroupId: data.ProjectGroupId,
            Name: data.Name,
            Description: data.Description,
            Visibility: data.Visibility,
            ReleasedOn: data.ReleasedOn);
    }

    public static ProjectDetailDto ToProjectDetailDto(ProjectInfo data)
    {
        return new ProjectDetailDto(
            Id: data.Id,
            ProjectGroupId: data.ProjectGroupId,
            ProjectGroupName: null,
            Genre: data.Genre,
            Name: data.Name,
            Description: data.Description,
            Visibility: data.Visibility,
            ReleasedOn: data.ReleasedOn,
            Crew: ImmutableArray<ProjectAuthorDto>.Empty,
            Cast: ImmutableArray<ProjectAuthorDto>.Empty,
            Artifacts: ImmutableArray<ProjectArtifactDto>.Empty,
            Blueprint: TemporaryProjectBlueprintMockup
        );
    }

    public static AuthorListDto ToAuthorListDto(AuthorInfo data)
    {
        return new AuthorListDto(
            Id: data.Id,
            Name: data.Name);
    }

    public static AuthorDetailDto ToAuthorDetailDto(AuthorInfo data)
    {
        return new AuthorDetailDto(
            Id: data.Id,
            Name: data.Name,
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
            Visibility: data.Visibility);
    }

    public static PlaylistDetailDto ToPlaylistDetailDto(PlaylistInfo data)
    {
        return new PlaylistDetailDto(
            Id: data.Id,
            Name: data.Name,
            Description: data.Description,
            Visibility: data.Visibility,
            Videos: data.VideoIds);
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
            ContainingProjectIds: data.ContainingProjectIds,
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
            MimeType: data.GetMimeType(),
            FileLength: data.FileLength,
            Duration: data.Duration,
            VideoStreams: data.VideoStreams.Select(ToVideoStreamDto).ToImmutableArray(),
            AudioStreams: data.AudioStreams.Select(ToAudioStreamDto).ToImmutableArray(),
            SubtitleStreams: data.SubtitleStreams.Select(ToSubtitleStreamDto).ToImmutableArray(),
            IsCorrupted: data.IsCorrupted);
    }

    public static VideoStreamDto ToVideoStreamDto(VideoInfo data)
    {
        return new VideoStreamDto(
            Codec: data.Codec,
            Bitrate: data.Bitrate,
            Width: data.Width,
            Height: data.Height,
            Framerate: data.Framerate);
    }

    public static AudioStreamDto ToAudioStreamDto(AudioInfo data)
    {
        return new AudioStreamDto(
            Codec: data.Codec,
            Bitrate: data.Bitrate,
            Channels: data.Channels,
            SampleRate: data.SampleRate);
    }

    public static SubtitleStreamDto ToSubtitleStreamDto(SubtitlesInfo data)
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
            Width: data.Width,
            Height: data.Height,
            Format: data.Format);
    }

    public static ShardDetailBaseDto ToShardDetailDto(ShardInfoBase data)
    {
        return data switch
        {
            VideoShardInfo v => ToVideoShardDetailDto(v),
            ImageShardInfo i => ToImageShardDetailDto(i),
            _ => throw new NotSupportedException($"Shards of '{data.GetType()}' are not supported.")
        };
    }

    public static TemporaryAccountInfoDto ToTemporaryAccountInfoDto(AccountInfo data)
    {
        return new TemporaryAccountInfoDto(
            Id: data.Id,
            EmailAddress: data.EmailAddress,
            PreferredCulture: data.PreferredCulture);
    }

    public static AccountDetailDto ToAccountDetailDto(
        AccountInfo data,
        IEnumerable<ProjectInfo> projects)
    {
        return new AccountDetailDto(
            Id: data.Id,
            Name: null,
            Uco: null,
            EmailAddress: data.EmailAddress,
            PreferredCulture: data.PreferredCulture,
            Projects: projects.Select(ToProjectListDto).ToImmutableArray(),
            Capabilities: data.Capabilities.Select(AccountCapability.Serialize).ToImmutableHashSet()
        );
    }
}
