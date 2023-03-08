using System;
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
            Description: LocalizedString.Create(
                (Const.InvariantCulture, "TODO"),
                (Const.CzechCulture,
@"Točíš filmy? Nebojíš se překračovat hranice? Hledáš důvod proč realizovat svůj nápad?
Tak to si tu správně! Natoč krátký film a přihlaš ho na 23. Filmový festival Fakulty Informatiky Masarykovy univerzity! Deadline je 9. dubna 2023, tak si pospěš!   
S námi můžeš ukázat svoji tvorbu před stovkami lidí a pokud tvůj snímek zaujme porotu nebo diváky, budeš odměněný i cenami.
Tak co, už přihlašuješ?
Jestli jsme tě ještě nepřesvědčili? Pro více technických a organizačních detailů si přečti pravidla (a v případě potíží s přihlašováním prosím kontaktuj festival-tech@fi.muni.cz).

Podmínky pro přijetí filmu na 23. Filmový festival Fakulty informatiky Masarykovy univerzity jsou:
1. Do soutěže mohou být přijata amatérská audiovizuální díla libovolného žánru i formy zpracování (hraná, animovaná) v délce do 8 minut.
2. Tvůrci filmu nesmí být profesionálové v tvorbě audiovizuálních děl.
3. Snímek nesmí být starší než 3 roky.
4. Snímek nesmí porušovat žádná autorská práva.
5. Film může přihlásit pouze jeden z jeho autorů, a to se souhlasem všech spoluautorů díla.

Povinné technické specifikace pro filmy:
- Formát (kodek) videa: H.264 (doporučený), MPEG-4 Part 2
- Formát (kodek) audia: WAV, FLAC, MP3 (bitrate u MP3 alespoň 192 kbps)
- Framerate videa: 24 fps
- Titulky: anglické ve formátu SRT nebo ASS
- Kontejner: MP4 (doporučené), MKV
- Rozlišení: na šířku alespoň FullHD (t.j. 1920)
- Bitrate: 10 - 20 Mbps
- Hlasitost: max. -3 dB
- Velikost každého souboru maximálně 2GB
- Pokud je potřeba platformě YouTube doložit, že držíte licenční práva na použitý materiál, kontaktujte nás na festival-tech@fi.muni.cz.

Úplné znění pravidel naleznete na http://festival.fi.muni.cz.

V případě technických problémů nás prosím kontaktujte na adrese: festival-tech@fi.muni.cz.
"
            )),
            ArtifactBlueprints: ImmutableArray.Create(
                new ProjectArtifactBlueprintDto(
                    Name: LocalizedString.Create(
                        (Const.InvariantCulture, "Film"),
                        (Const.CzechCulture, "Film")
                    ),
                    Description: LocalizedString.Create(
                        (Const.InvariantCulture, "TODO"),
                        (Const.CzechCulture, "Soubor s filmem splňující technické požadavky")
                    ),
                    SlotName: Const.FilmBlueprintSlot,
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
                    Description: LocalizedString.Create(
                        (Const.InvariantCulture, "TODO"),
                        (Const.CzechCulture, "Volitelné představení filmu jeho tvůrci o délce maximálně 30 " +
                        "sekund. Technické požadavky jsou stejné jako u filmu.")
                    ),
                    SlotName: Const.VideoAnnotationBlueprintSlot,
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
                    Description: LocalizedString.Create(
                        (Const.InvariantCulture, "TODO"),
                        (Const.CzechCulture, "Nahrajte kvalitní screenshoty nebo fotografie ze scén, které váš film vystihují. Budou použity v medailonku vašeho filmu a do brožury festivalu.")
                    ),
                    SlotName: Const.CoverPhotoBlueprintSlot,
                    Arity: new ArgumentArity(Const.CoverPhotoMinCount, Const.CoverPhotoMaxCount),
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
            Name: (LocalizedString)data.Name,
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
            Visibility: data.Visibility);
    }

    public static AuthorDetailDto ToAuthorDetailDto(AuthorInfo data)
    {
        return new AuthorDetailDto(
            Id: data.Id,
            Name: data.Name,
            Visibility: data.Visibility,
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
            FileExtension: data.FileExtension,
            FormatName: data.FormatName,
            MimeType: data.MimeType,
            Width: data.Width,
            Height: data.Height,
            IsCorrupted: data.IsCorrupted);
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
            Capabilities: data.Capabilities
        );
    }
}
