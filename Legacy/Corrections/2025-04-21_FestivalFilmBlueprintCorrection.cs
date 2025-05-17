#pragma warning disable 0618

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
- [ ] StringLength [1, 42]

Description:
- [ ] StringLength [50, 200]

Genre:
- [ ] StringLength [1, 32]

Videos:
- [ ] MediaDuration [00:00:01, 00:08:00] +- 00:00:01
- [ ] FileLength [0, 2 GiB]
- [ ] MediaBitrate [10 Mbps, 20 Mbps] +- 1 Mbps
- [ ] MediaShorterSide [1080, inf]
- [ ] MimeType {video/mp4, video/x-matroska}
- [ ] MediaStreamCount (Video) [1, 1]
- [ ] MediaStreamCount (Audio) [1, 1]
- [ ] VideoSubtitles
- [ ] MediaStreamCodec (Video) {h264, mpeg4}
- [ ] MediaStreamCodec (Audio) {aac, mp3, flac}
- [ ] MediaStreamCodec (Subtitles) {subrip, ass}
- [ ] MediaBitrate (Audio) [192 Kbps, inf]
- [ ] VideoFramerate {24 fps, 25 fps}


Video-annotation:
- [ ] MediaDuration [00:00:01, 00:30:00] +- 00:00:01
- [ ] FileLength [0, 512 MiB]
- [ ] MediaBitrate [10 Mbps, 20 Mbps] +- 1 Mbps
- [ ] MediaShorterSide [1080, inf]
- [ ] MimeType {video/mp4, video/x-matroska}
- [ ] MediaStreamCount (Video) [1, 1]
- [ ] MediaStreamCount (Audio) [1, 1]
- [ ] VideoSubtitles
- [ ] MediaStreamCodec (Video) {h264, mpeg4}
- [ ] MediaStreamCodec (Subtitles) {subrip, ass}
- [ ] VideoFramerate {24 fps, 25 fps}

Cover photo:
- [ ] MediaShorterSide [1080, inf]
- [ ] MediaAspectRatio [4:3, 16:9]
- [ ] MimeType {video/jpeg, video/png}

Crew:
- [ ] Type {core:author-ref[]}
- [ ] ArrayLength [1, inf]

Cast:
- [ ] Type {core:author-ref[]}
- [ ] ArrayLength [1, inf]

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
                                new ShardMetadataTypeRequirement([KafeType.Parse("media:shard/video")])
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
                ["film"] = new(
                    name: LocalizedString.Create(
                        (Const.InvariantCulture, "Film file"),
                        (Const.CzechCulture, "Soubor s filmem")
                    ),
                    requirements: [..kof.WrapMany(
                        new ShardMetadataTypeRequirement(
                            [KafeType.Parse("media:shard/video")]
                        ),
                        new VideoSubtitlesRequirement(
                            Const.EnglishCulture.TwoLetterISOLanguageName
                        )
                    )]
                ),
                ["video-annotation"] = new(
                    name: LocalizedString.Create(
                        (Const.InvariantCulture, "Video-Annotation"),
                        (Const.CzechCulture, "Videoanotace")
                    ),
                    requirements: [..kof.WrapMany(
                        new ShardMetadataTypeRequirement(
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
                        )
                    )]
                ),
                ["cover-photos"] = new(
                    name: LocalizedString.Create(
                        (Const.InvariantCulture, "Cover photos"),
                        (Const.CzechCulture, "Titulní fotografie")
                    ),
                    requirements: [..kof.WrapMany(
                        new ShardMetadataTypeRequirement(
                            [KafeType.Parse("core:shard-ref[]")]
                        ),
                        new ArrayLengthRequirement(
                            Min: 1,
                            Max: 5
                        ),
                        new AllRequirement(
                            Requirements: [..kof.WrapMany(
                                new ShardMetadataTypeRequirement(
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
