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

// TODO: Uncomment once correction is ready
// [AutoCorrection("2025-04-21")]
public class FestivalFilmBlueprintCorrection : IEventCorrection
{
    private readonly ILogger<FestivalFilmBlueprintCorrection> logger;

    public FestivalFilmBlueprintCorrection(ILogger<FestivalFilmBlueprintCorrection> logger)
    {
        this.logger = logger;
    }

    public static readonly Hrib WmaBlueprintId = Hrib.Parse("lgc-wma-proj");

    public static readonly BlueprintInfo WmaProjectBlueprint = new(
        id: WmaBlueprintId.ToString(),
        name: LocalizedString.CreateInvariant("WMA Project (legacy)"),
        properties: new Dictionary<string, BlueprintProperty>
        {
            ["videos"] = new(
                name: LocalizedString.Create(
                    (Const.InvariantCulture, "Film files"),
                    (Const.CzechCulture, "Soubory s filmy")
                ),
                requirements: [
                    new KafeObject(
                        KafeType.Parse("core:req/type"),
                        new TypeRequirement(
                            Include: [KafeType.Parse("core:shard-ref[]")],
                            Exclude: []
                        )
                    ),
                    new KafeObject(
                        KafeType.Parse("core:req/all"),
                        new AllRequirement(
                            Requirements: [
                                new KafeObject(
                                    KafeType.Parse("core:req/shard-metadata-type"),
                                    new ShardMetadataTypeRequirement(
                                        [KafeType.Parse("media:shard/video")]
                                    )
                                )
                            ]
                        )
                    ),
                ]
            )
        }.ToImmutableDictionary()
    );

    public static readonly Hrib FestivalRegistrationBlueprintId = Hrib.Parse("lgc-bp-freg");

    public static readonly BlueprintInfo FestivalRegistrationBlueprint = new(
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
                requirements: [
                    new KafeObject(
                        KafeType.Parse("core:req/shard-metadata-type"),
                        new ShardMetadataTypeRequirement(
                            [KafeType.Parse("media:shard/video")]
                        )
                    ),
                    new KafeObject(
                        KafeType.Parse("media:req/video-subtitles"),
                        new VideoSubtitlesRequirement(
                            Const.EnglishCulture.TwoLetterISOLanguageName
                        )
                    ),
                ]
            ),
            ["video-annotation"] = new(
                name: LocalizedString.Create(
                    (Const.InvariantCulture, "Video-Annotation"),
                    (Const.CzechCulture, "Videoanotace")
                ),
                requirements: [
                    new KafeObject(
                        KafeType.Parse("core:req/shard-metadata-type"),
                        new ShardMetadataTypeRequirement(
                            [KafeType.Parse("media:shard/video")]
                        )
                    ),
                    new KafeObject(
                        KafeType.Parse("core:req/shard-mime-type"),
                        new ShardMimeTypeRequirement(
                            Include: ["video/mp4", "video/x-matroska"],
                            Exclude: []
                        )
                    ),
                    new KafeObject(
                        KafeType.Parse("core:req/shard-file-length"),
                        new ShardFileLengthRequirement(
                            Min: null,
                            Max: 512 << 20
                        )
                    ),
                    new KafeObject(
                        KafeType.Parse("media:req/video-subtitles"),
                        new VideoSubtitlesRequirement(
                            Const.EnglishCulture.TwoLetterISOLanguageName
                        )
                    ),
                    new KafeObject(
                        KafeType.Parse("media:req/stream-codec"),
                        new MediaStreamCodecRequirement(
                            Include: ["subrip", "ass"],
                            Exclude: [],
                            Kind: MediaStreamKind.Subtitles
                        )
                    ),
                    new KafeObject(
                        KafeType.Parse("media:req/stream-codec"),
                        new MediaStreamCodecRequirement(
                            Include: ["aac", "mp3", "flac"],
                            Exclude: [],
                            Kind: MediaStreamKind.Audio
                        )
                    ),
                    new KafeObject(
                        KafeType.Parse("media:req/stream-codec"),
                        new MediaStreamCodecRequirement(
                            Include: ["h264", "mpeg4"],
                            Exclude: [],
                            Kind: MediaStreamKind.Video
                        )
                    ),
                    new KafeObject(
                        KafeType.Parse("media:req/bitrate"),
                        new MediaBitrateRequirement(
                            Min: 10_000_000,
                            Max: 20_000_000,
                            Kind: MediaBitrateKind.Total
                        )
                    ),
                    new KafeObject(
                        KafeType.Parse("media:req/video-framerate"),
                        new VideoFramerateRequirement(
                            Include: [24.0, 25.0],
                            Exclude: []
                        )
                    )
                ]
            ),
            ["cover-photos"] = new(
                name: LocalizedString.Create(
                    (Const.InvariantCulture, "Cover photos"),
                    (Const.CzechCulture, "Titulní fotografie")
                ),
                requirements: [
                    new KafeObject(
                        KafeType.Parse("core:req/type"),
                        new ShardMetadataTypeRequirement(
                            [KafeType.Parse("core:shard-ref[]")]
                        )
                    ),
                    new KafeObject(
                        KafeType.Parse("core:req/array-length"),
                        new ArrayLengthRequirement(
                            Min: 1,
                            Max: 5
                        )
                    ),
                    new KafeObject(
                        KafeType.Parse("core:req/all"),
                        new AllRequirement(
                            Requirements: [
                                new KafeObject(
                                    KafeType.Parse("core:req/shard-metadata-type"),
                                    new ShardMetadataTypeRequirement(
                                        [KafeType.Parse("media:shard/image")]
                                    )
                                ),
                                new KafeObject(
                                    KafeType.Parse("core:req/shard-mime-type"),
                                    new ShardMimeTypeRequirement(
                                        Include: ["image/jpeg", "image/png"],
                                        Exclude: []
                                    )
                                ),
                                new KafeObject(
                                    KafeType.Parse("media:req/shorter-side"),
                                    new MediaShorterSideRequirement(1080)
                                ),
                                new KafeObject(
                                    KafeType.Parse("media:req/aspect-ratio"),
                                    new MediaAspectRatioRequirement("4:3", "16:9")
                                )
                            ]
                        )
                    ),
                ]
            )
        }.ToImmutableDictionary()
    );

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
}
