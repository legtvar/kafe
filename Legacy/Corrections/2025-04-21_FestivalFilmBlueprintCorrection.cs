#pragma warning disable 0618

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Core.Requirements;
using Kafe.Data;
using Kafe.Data.Aggregates;
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

    public static readonly Hrib FilmBlueprintId = Hrib.Parse("lgc-bp-film");

    public static readonly BlueprintInfo FilmBlueprint = new(
        id: FilmBlueprintId.ToString(),
        name: LocalizedString.CreateInvariant("Film (legacy)"),
        properties: new Dictionary<string, BlueprintProperty>
        {
            ["video"] = new(
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
                    )
                ]
            ),
            ["subtitles"] = new(
                name: LocalizedString.Create(
                    LocalizedString.Create(
                        (Const.InvariantCulture, "Subtitles"),
                        (Const.CzechCulture, "Titulky")
                    )
                ),
                requirements: [
                    new KafeObject(
                        KafeType.Parse("core:req/shard-metadata-type"),
                        new ShardMetadataTypeRequirement(
                            [KafeType.Parse("media:shard/subtitles")]
                        )
                    )
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
                    (Const.InvariantCulture, "Film"),
                    (Const.CzechCulture, "Film")
                )
            ),
            ["video-annotation"] = new(
                name: LocalizedString.Create(
                    (Const.InvariantCulture, "Video-Annotation"),
                    (Const.CzechCulture, "Videoanotace")
                )
            ),
            ["cover-photos"] = new(
                name: LocalizedString.Create(
                    (Const.InvariantCulture, "Cover Photos"),
                    (Const.CzechCulture, "Titulní fotografie")
                )
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
