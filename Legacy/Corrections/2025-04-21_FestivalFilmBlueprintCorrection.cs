#pragma warning disable 0618

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Core.Requirements;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Media;
using Kafe.Media.Requirements;
using Marten;
using Microsoft.Extensions.Logging;

namespace Kafe.Legacy.Corrections;

/*
Checklist
---------

Name:
- [x] StringLength [1, 42]

Description:
- [x] StringLength [50, 200]

Genre:
- [x] StringLength [1, 32]

Videos:
- [x] MediaDuration [00:00:01, 00:08:00] +- 00:00:01
- [x] FileLength [0, 2 GiB]
- [x] MediaBitrate [10 Mbps, 20 Mbps] +- 1 Mbps
- [x] MediaShorterSide [1080, inf]
- [x] MimeType {video/mp4, video/x-matroska}
- [x] MediaStreamCount (Video) [1, 1]
- [x] MediaStreamCount (Audio) [1, 1]
- [x] VideoSubtitles {en}
- [x] MediaStreamCodec (Video) {h264, mpeg4}
- [x] MediaStreamCodec (Audio) {aac, mp3, flac}
- [x] MediaStreamCodec (Subtitles) {subrip, ass}
- [x] MediaBitrate (Audio) [192 Kbps, inf]
- [x] VideoFramerate {24 fps, 25 fps}

Video-annotation:
- [x] MediaDuration [00:00:01, 00:30:00] +- 00:00:01
- [x] FileLength [0, 512 MiB]
- [x] MediaBitrate [10 Mbps, 20 Mbps] +- 1 Mbps
- [x] MediaShorterSide [1080, inf]
- [x] MimeType {video/mp4, video/x-matroska}
- [x] MediaStreamCount (Video) [1, 1]
- [x] MediaStreamCount (Audio) [1, 1]
- [x] VideoSubtitles {en}
- [x] MediaStreamCodec (Video) {h264, mpeg4}
- [x] MediaStreamCodec (Subtitles) {subrip, ass}
- [x] VideoFramerate {24 fps, 25 fps}

Cover photo:
- [x] MediaShorterSide [1080, inf]
- [x] MediaAspectRatio [4:3, 16:9]
- [x] MimeType {video/jpeg, video/png}

Crew:
- [x] Type {core:author-ref[]}

Cast:
- [x] Type {core:author-ref[]}

Review:
- [ ] Stage {info, file, tech-review, visual-review, dramaturgy-review}
*/


// TODO: Uncomment once correction is ready
// [AutoCorrection("2025-04-21")]
public class FestivalFilmBlueprintCorrection : IEventCorrection
{
    private readonly ILogger<FestivalFilmBlueprintCorrection> logger;
    private readonly KafeObjectFactory objectFactory;

    public FestivalFilmBlueprintCorrection(
        ILogger<FestivalFilmBlueprintCorrection> logger,
        KafeObjectFactory objectFactory
    )
    {
        this.logger = logger;
        this.objectFactory = objectFactory;
    }

    public static readonly Hrib WmaBlueprintId = Hrib.Parse("lgc-wma-proj");

    public static readonly Hrib FestivalRegistrationBlueprintId = Hrib.Parse("lgc-bp-freg");

    public async Task Apply(IDocumentSession db, CancellationToken ct = default)
    {
        var projects = (await db.Query<ProjectInfo>().ToListAsync(ct)).ToImmutableArray();
        if (projects.IsDefaultOrEmpty)
        {
            logger.LogInformation("No projects found. No blueprints will be added and nothing converted.");
            return;
        }

        logger.LogInformation("{Count} projects found. Adding blueprints.", projects.Length);
    }

    public static BlueprintInfo GetWmaProjectBlueprint(KafeObjectFactory kof)
    {
        return new(
            id: WmaBlueprintId.ToString(),
            name: LocalizedString.CreateInvariant("WMA Project (legacy)"),
            properties: new Dictionary<string, BlueprintProperty>
            {
                ["name"] = new(
                    name: LocalizedString.Create(
                        (Const.InvariantCulture, "Project name"),
                        (Const.CzechCulture, "Název projektu")
                    ),
                    requirements: [..kof.WrapMany(
                        new TypeRequirement(
                            Include: [KafeType.Parse("core:localized-string")],
                            Exclude: []
                        )
                    )]
                ),
                ["description"] = new(
                    name: LocalizedString.Create(
                        (Const.InvariantCulture, "Project description"),
                        (Const.CzechCulture, "Popis projektu")
                    ),
                    requirements: [..kof.WrapMany(
                        new TypeRequirement(
                            Include: [KafeType.Parse("core:localized-string")],
                            Exclude: []
                        )
                    )]
                ),
                ["genre"] = new(
                    name: LocalizedString.Create(
                        (Const.InvariantCulture, "Genre"),
                        (Const.CzechCulture, "Žánr")
                    ),
                    requirements: [..kof.WrapMany(
                        new TypeRequirement(
                            Include: [KafeType.Parse("core:localized-string")],
                            Exclude: []
                        )
                    )]
                ),
                ["cast"] = new(
                    name: LocalizedString.Create(
                        (Const.InvariantCulture, "Cast"),
                        (Const.CzechCulture, "Herci")
                    ),
                    requirements: [..kof.WrapMany(
                        new TypeRequirement(
                            Include: [KafeType.Parse("core:author-ref[]")],
                            Exclude: []
                        )
                    )]
                ),
                ["crew"] = new(
                    name: LocalizedString.Create(
                        (Const.InvariantCulture, "Crew"),
                        (Const.CzechCulture, "Štáb")
                    ),
                    requirements: [..kof.WrapMany(
                        new TypeRequirement(
                            Include: [KafeType.Parse("core:author-ref[]")],
                            Exclude: []
                        )
                    )]
                ),
                ["videos"] = new(
                    name: LocalizedString.Create(
                        (Const.InvariantCulture, "Video files"),
                        (Const.CzechCulture, "Video soubory")
                    ),
                    requirements: [..kof.WrapMany(
                        new TypeRequirement(
                            Include: [KafeType.Parse("core:shard-ref[]")],
                            Exclude: []
                        ),
                        new AllRequirement(
                            Requirements: [..kof.WrapMany(
                                new ShardPayloadTypeRequirement([KafeType.Parse("media:shard/video")])
                            )]
                        )
                    )]
                )
            }.ToImmutableDictionary()
        );
    }

    public static BlueprintInfo GetFestivalProjectBlueprint(KafeObjectFactory kof)
    {
        return new(
            id: FestivalRegistrationBlueprintId.ToString(),
            name: LocalizedString.Create(
                (Const.InvariantCulture, "Film Festival Registration (legacy)"),
                (Const.CzechCulture, "Přihláška na filmový festival (legacy)")
            ),
            properties: new Dictionary<string, BlueprintProperty>
            {
                ["name"] = new(
                    name: LocalizedString.Create(
                        (Const.InvariantCulture, "Film title"),
                        (Const.CzechCulture, "Název filmu")
                    ),
                    requirements: [..kof.WrapMany(
                        new TypeRequirement(
                            Include: [KafeType.Parse("core:localized-string")],
                            Exclude: []
                        ),
                        new StringLengthRequirement(1, 42)
                    )]
                ),
                ["description"] = new(
                    name: LocalizedString.Create(
                        (Const.InvariantCulture, "Film description"),
                        (Const.CzechCulture, "Popis filmu")
                    ),
                    requirements: [..kof.WrapMany(
                        new TypeRequirement(
                            Include: [KafeType.Parse("core:localized-string")],
                            Exclude: []
                        ),
                        new StringLengthRequirement(50, 200)
                    )]
                ),
                ["genre"] = new(
                    name: LocalizedString.Create(
                        (Const.InvariantCulture, "Genre"),
                        (Const.CzechCulture, "Žánr")
                    ),
                    requirements: [..kof.WrapMany(
                        new TypeRequirement(
                            Include: [KafeType.Parse("core:localized-string")],
                            Exclude: []
                        ),
                        new StringLengthRequirement(1, 32)
                    )]
                ),
                ["cast"] = new(
                    name: LocalizedString.Create(
                        (Const.InvariantCulture, "Cast"),
                        (Const.CzechCulture, "Herci")
                    ),
                    requirements: [..kof.WrapMany(
                        new TypeRequirement(
                            Include: [KafeType.Parse("core:author-ref[]")],
                            Exclude: []
                        )
                    )]
                ),
                ["crew"] = new(
                    name: LocalizedString.Create(
                        (Const.InvariantCulture, "Crew"),
                        (Const.CzechCulture, "Štáb")
                    ),
                    requirements: [..kof.WrapMany(
                        new TypeRequirement(
                            Include: [KafeType.Parse("core:author-ref[]")],
                            Exclude: []
                        )
                    )]
                ),
                ["film"] = new(
                    name: LocalizedString.Create(
                        (Const.InvariantCulture, "Film file"),
                        (Const.CzechCulture, "Soubor s filmem")
                    ),
                    requirements: [..kof.WrapMany(
                        new ShardPayloadTypeRequirement(
                            [KafeType.Parse("media:shard/video")]
                        ),
                        new ShardMimeTypeRequirement(
                            Include: ["video/mp4", "video/x-matroska"],
                            Exclude: []
                        ),
                        new ShardFileLengthRequirement(
                            Min: null,
                            Max: 2 << 30
                        ),
                        new VideoSubtitlesRequirement(
                            Const.EnglishCulture.TwoLetterISOLanguageName
                        ),
                        new MediaStreamCodecRequirement(
                            Include: ["subrip", "ass"],
                            Exclude: [],
                            Kind: MediaStreamKind.Subtitles
                        ),
                        new MediaStreamCodecRequirement(
                            Include: ["aac", "mp3", "flac"],
                            Exclude: [],
                            Kind: MediaStreamKind.Audio
                        ),
                        new MediaStreamCodecRequirement(
                            Include: ["h264", "mpeg4"],
                            Exclude: [],
                            Kind: MediaStreamKind.Video
                        ),
                        new MediaBitrateRequirement(
                            Min: 10_000_000,
                            Max: 20_000_000,
                            Kind: MediaBitrateKind.Total
                        ),
                        new MediaBitrateRequirement(
                            Min: 192_000,
                            Max: null,
                            Kind: MediaBitrateKind.Audio
                        ),
                        new VideoFramerateRequirement(
                            Include: [24.0, 25.0],
                            Exclude: []
                        ),
                        new MediaDurationRequirement(
                            Min: null,
                            Max: TimeSpan.FromMinutes(8)
                        ),
                        new MediaShorterSideRequirement(1080),
                        new MediaStreamCountRequirement(1, 1, MediaStreamKind.Video),
                        new MediaStreamCountRequirement(1, 1, MediaStreamKind.Audio)
                    )]
                ),
                ["video-annotation"] = new(
                    name: LocalizedString.Create(
                        (Const.InvariantCulture, "Video-Annotation"),
                        (Const.CzechCulture, "Videoanotace")
                    ),
                    requirements: [..kof.WrapMany(
                        new ShardPayloadTypeRequirement(
                            [KafeType.Parse("media:shard/video")]
                        ),
                        new ShardMimeTypeRequirement(
                            Include: ["video/mp4", "video/x-matroska"],
                            Exclude: []
                        ),
                        new ShardFileLengthRequirement(
                            Min: null,
                            Max: 512 << 20
                        ),
                        new VideoSubtitlesRequirement(
                            Const.EnglishCulture.TwoLetterISOLanguageName
                        ),
                        new MediaStreamCodecRequirement(
                            Include: ["subrip", "ass"],
                            Exclude: [],
                            Kind: MediaStreamKind.Subtitles
                        ),
                        new MediaStreamCodecRequirement(
                            Include: ["aac", "mp3", "flac"],
                            Exclude: [],
                            Kind: MediaStreamKind.Audio
                        ),
                        new MediaStreamCodecRequirement(
                            Include: ["h264", "mpeg4"],
                            Exclude: [],
                            Kind: MediaStreamKind.Video
                        ),
                        new MediaBitrateRequirement(
                            Min: 10_000_000,
                            Max: 20_000_000,
                            Kind: MediaBitrateKind.Total
                        ),
                        new VideoFramerateRequirement(
                            Include: [24.0, 25.0],
                            Exclude: []
                        ),
                        new MediaDurationRequirement(
                            Min: null,
                            Max: TimeSpan.FromSeconds(30)
                        ),
                        new MediaShorterSideRequirement(1080),
                        new MediaStreamCountRequirement(1, 1, MediaStreamKind.Video),
                        new MediaStreamCountRequirement(1, 1, MediaStreamKind.Audio)
                    )]
                ),
                ["cover-photos"] = new(
                    name: LocalizedString.Create(
                        (Const.InvariantCulture, "Cover photos"),
                        (Const.CzechCulture, "Titulní fotografie")
                    ),
                    requirements: [..kof.WrapMany(
                        new ShardPayloadTypeRequirement(
                            [KafeType.Parse("core:shard-ref[]")]
                        ),
                        new ArrayLengthRequirement(
                            Min: 1,
                            Max: 5
                        ),
                        new AllRequirement(
                            Requirements: [..kof.WrapMany(
                                new ShardPayloadTypeRequirement(
                                    [KafeType.Parse("media:shard/image")]
                                ),
                                new ShardMimeTypeRequirement(
                                    Include: ["image/jpeg", "image/png"],
                                    Exclude: []
                                ),
                                new MediaShorterSideRequirement(1080),
                                new MediaAspectRatioRequirement("4:3", "16:9")
                            )]
                        )
                    )]
                )
            }.ToImmutableDictionary()
        );
    }
}
