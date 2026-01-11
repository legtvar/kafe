using System.Collections.Immutable;

namespace Kafe.Data.Aggregates;

public record ProjectBlueprint
{
    public ImmutableArray<string> RequiredReviewers { get; init; } = ImmutableArray<string>.Empty;

    public ImmutableDictionary<string, ProjectArtifactBlueprint> ArtifactBlueprints { get; init; }
        = ImmutableDictionary<string, ProjectArtifactBlueprint>.Empty;

    //  DEFAULT PROJECT BLUEPRINT
    public static readonly ProjectBlueprint TemporaryProjectBlueprint = new ProjectBlueprint {
        RequiredReviewers = [Const.TechReviewer, Const.VisualReviewer, Const.DramaturgyReviewer],
        ArtifactBlueprints = ImmutableDictionary<string, ProjectArtifactBlueprint>.Empty
            // Film
            .Add(Const.FilmBlueprintSlot,
                new ProjectArtifactBlueprint
                {
                    Name = LocalizedString.Create(
                        (Const.InvariantCulture, "Film"),
                        (Const.CzechCulture, "Film"),
                        (Const.SlovakCulture, "Film")
                    ),
                    Description = null,
                    Arity = ArgumentArity.ExactlyOne,

                    ShardBlueprints = ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint>.Empty
                        .Add(ShardKind.Video,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Film file"),
                                    (Const.CzechCulture, "Soubor s filmem"),
                                    (Const.SlovakCulture, "Súbor s filmom")
                                ),
                                Description = null,
                                Arity = ArgumentArity.ExactlyOne
                            })
                        .Add(ShardKind.Subtitles,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Subtitles"),
                                    (Const.CzechCulture, "Titulky"),
                                    (Const.SlovakCulture, "Titulky")
                                ),
                                Description = LocalizedString.Create(
                                    (
                                        Const.InvariantCulture,
                                        "English subtitles if the film is in Czech or Slovak, " +
                                        "or Czech/Slovak subtitles if the film is in English."
                                    ),
                                    (
                                        Const.CzechCulture,
                                        "Anglické titulky, pokud je film v češtině nebo slovenštině, " +
                                        "nebo české/slovenské titulky, pokud je film v angličtině."
                                    ),
                                    (
                                        Const.SlovakCulture,
                                        "Anglické titulky, ak je film v češtine alebo slovenčine, " +
                                        "alebo české/slovenské titulky, ak je film v angličtine."
                                    )
                                ),
                                Arity = ArgumentArity.ExactlyOne
                            })
                }
            )
            // Video Annotation
            .Add(Const.VideoAnnotationBlueprintSlot,
                new ProjectArtifactBlueprint
                {
                    Name = LocalizedString.Create(
                        (Const.InvariantCulture, "Video-annotation"),
                        (Const.CzechCulture, "Videoanotace"),
                        (Const.SlovakCulture, "Videoanotácia")
                    ),
                    Description = null,
                    Arity = ArgumentArity.ZeroOrOne,

                    ShardBlueprints = ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint>.Empty
                        .Add(ShardKind.Video,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Video-annotation file"),
                                    (Const.CzechCulture, "Soubor s videoanotací"),
                                    (Const.SlovakCulture, "Súbor s videoanotáciou")
                                ),
                                Description = null,
                                Arity = ArgumentArity.ExactlyOne
                            })
                        .Add(ShardKind.Subtitles,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Subtitles"),
                                    (Const.CzechCulture, "Titulky"),
                                    (Const.SlovakCulture, "Titulky")
                                ),
                                Description = LocalizedString.Create(
                                    (
                                        Const.InvariantCulture,
                                        "English subtitles if the film is in Czech or Slovak, " +
                                        "or Czech/Slovak subtitles if the film is in English."
                                    ),
                                    (
                                        Const.CzechCulture,
                                        "Anglické titulky, pokud je film v češtině nebo slovenštině, " +
                                        "nebo české/slovenské titulky, pokud je film v angličtině."
                                    ),
                                    (
                                        Const.SlovakCulture,
                                        "Anglické titulky, ak je film v češtine alebo slovenčine, " +
                                        "alebo české/slovenské titulky, ak je film v angličtine."
                                    )
                                ),
                                Arity = ArgumentArity.ExactlyOne
                            })
                }
            )
            // Cover Photo
            .Add(Const.CoverPhotoBlueprintSlot,
                new ProjectArtifactBlueprint
                {
                    Name = LocalizedString.Create(
                        (Const.InvariantCulture, "Annotation photographs"),
                        (Const.CzechCulture, "Anotační fotografie"),
                        (Const.SlovakCulture, "Anotačné fotografie")
                    ),
                    Description = null,
                    Arity = new ArgumentArity(Const.CoverPhotoMinCount, Const.CoverPhotoMaxCount),
                    ShardBlueprints = ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint>.Empty
                        .Add(ShardKind.Image,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Annotation photograph file"),
                                    (Const.CzechCulture, "Soubor s anotační fotografií"),
                                    (Const.SlovakCulture, "Súbor s anotačnou fotografiou")
                                ),
                                Description = null,
                                Arity = ArgumentArity.ExactlyOne
                            })
                }
            )
    };

    // LEMMA PROJECT BLUEPRINT
    public static readonly ProjectBlueprint TemporaryLemmaProjectBlueprint = new ProjectBlueprint {
        RequiredReviewers = [Const.TechReviewer, Const.VisualReviewer, Const.DramaturgyReviewer],
        ArtifactBlueprints = ImmutableDictionary<string, ProjectArtifactBlueprint>.Empty
            // Film
            .Add(Const.FilmBlueprintSlot,
                new ProjectArtifactBlueprint
                {
                    Name = LocalizedString.Create(
                        (Const.InvariantCulture, "Film"),
                        (Const.CzechCulture, "Film"),
                        (Const.SlovakCulture, "Film")
                    ),
                    Description = LocalizedString.Create(
                        (Const.InvariantCulture, "Your film."),
                        (Const.CzechCulture, "Váš film."),
                        (Const.SlovakCulture, "Váš film.")
                    ),
                    Arity = ArgumentArity.ExactlyOne,

                    ShardBlueprints = ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint>.Empty
                        .Add(ShardKind.Video,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Film file"),
                                    (Const.CzechCulture, "Soubor s filmem"),
                                    (Const.SlovakCulture, "Súbor s filmom")
                                ),
                                Description = LocalizedString.Create(
                                    (
                                        Const.InvariantCulture,
                                        "The film must strictly follow our mandatory technical specifications that " +
                                        "can be found on the bottom of our registration page in the 'Film' section."
                                    ),
                                    (
                                        Const.CzechCulture,
                                        "Film musí striktně splňovat naše povinné technické specifikace, které " +
                                        "naleznete ve spodní části naší registrační stránky v sekci 'Film'."
                                    ),
                                    (
                                        Const.SlovakCulture,
                                        "Film musí striktne spĺňať naše povinné technické špecifikácie, ktoré " +
                                        "nájdete v spodnej časti našej registračnej stránky v sekcii 'Film'."
                                    )
                                ),
                                Arity = ArgumentArity.ExactlyOne
                            })
                        .Add(ShardKind.Subtitles,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Subtitles"),
                                    (Const.CzechCulture, "Titulky"),
                                    (Const.SlovakCulture, "Titulky")
                                ),
                                Description = LocalizedString.Create(
                                    (
                                        Const.InvariantCulture,
                                        "English subtitles if the film is in Czech or Slovak, " +
                                        "or Czech/Slovak subtitles if the film is in English. " +
                                        "The only accepted format is SRT."
                                    ),
                                    (
                                        Const.CzechCulture,
                                        "Anglické titulky, pokud je film v češtině nebo slovenštině, " +
                                        "nebo české/slovenské titulky, pokud je film v angličtině. " +
                                        "Jediný akceptovaný formát je SRT."
                                    ),
                                    (
                                        Const.SlovakCulture,
                                        "Anglické titulky, ak je film v češtine alebo slovenčine, " +
                                        "alebo české/slovenské titulky, ak je film v angličtine. " +
                                        "Jediný akceptovaný formát je SRT."
                                    )
                                ),
                                Arity = ArgumentArity.ExactlyOne
                            })
                }
            )
            // Annotation Photographs
            .Add(Const.CoverPhotoBlueprintSlot,
                new ProjectArtifactBlueprint
                {
                    Name = LocalizedString.Create(
                        (Const.InvariantCulture, "Annotation photographs"),
                        (Const.CzechCulture, "Anotační fotografie"),
                        (Const.SlovakCulture, "Anotačné fotografie")
                    ),
                    Description = LocalizedString.Create(
                        (
                            Const.InvariantCulture,
                            "Annotation photographs of your film. Choose photographs that will represent your film " +
                            "in our digital and printed festival graphics. These can be either screenshots of the " +
                            "film scenes or your own photographs. Please, only insert photographs without texts and frames!"
                        ),
                        (
                            Const.CzechCulture,
                            "Anotační fotografie vašeho filmu. Vyberte si fotografie, které budou váš film " +
                            "reprezentovat v našich digitálních a tištěných festivalových grafikách. Mohou " +
                            "to být buď snímky obrazovky filmových scén, nebo vaše vlastní fotografie. Vkládejte " +
                            "prosím pouze fotografie bez textů a rámečků!"
                        ),
                        (
                            Const.SlovakCulture,
                            "Anotačné fotografie vášho filmu. Vyberte si fotografie, ktoré budú váš film " +
                            "reprezentovať v našich digitálnych a tlačených festivalových grafikách. Môžu " +
                            "to byť buď snímky obrazovky filmových scén, alebo vaše vlastné fotografie. Vkladajte " +
                            "prosím iba fotografie bez textov a rámčekov!"
                        )
                    ),
                    Arity = new ArgumentArity(Const.LemmaCoverPhotoMinCount, Const.CoverPhotoMaxCount),
                    ShardBlueprints = ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint>.Empty
                        .Add(ShardKind.Image,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Annotation photograph file"),
                                    (Const.CzechCulture, "Soubor s anotační fotografií"),
                                    (Const.SlovakCulture, "Súbor s anotačnou fotografiou")
                                ),
                                Description = LocalizedString.Create(
                                    (
                                        Const.InvariantCulture,
                                        "The annotation photographs must strictly follow our mandatory technical " +
                                        "specifications that can be found on the bottom of our registration page in " +
                                        "the 'Annotated photographs' section."
                                    ),
                                    (
                                        Const.CzechCulture,
                                        "Anotační fotografie musí striktně splňovat naše povinné technické specifikace, " +
                                        "které naleznete ve spodní části naší registrační stránky v sekci 'Anotační fotografie'."
                                    ),
                                    (
                                        Const.SlovakCulture,
                                        "Anotačné fotografie musia striktne spĺňať naše povinné technické špecifikácie, " +
                                        "ktoré nájdete v spodnej časti našej registračnej stránky v sekcii 'Anotační fotografie'."
                                    )
                                ),
                                Arity = ArgumentArity.ExactlyOne
                            })
                }
            )
            // BTS Photographs
            .Add(Const.BTSPhotoBlueprintSlot,
                new ProjectArtifactBlueprint
                {
                    Name = LocalizedString.Create(
                        (Const.InvariantCulture, "Behind-the-scenes photographs"),
                        (Const.CzechCulture, "Fotografie ze zákulisí"),
                        (Const.SlovakCulture, "Fotografie zo zákulisia")
                    ),
                    Description = LocalizedString.Create(
                        (
                            Const.InvariantCulture,
                            "Behind-the-scenes photographs from the filming of your film. (Optional)"
                        ),
                        (
                            Const.CzechCulture,
                            "Fotografie ze zákulisí natáčení vašeho filmu. (Volitelné)"
                        ),
                        (
                            Const.SlovakCulture,
                            "Fotografie zo zákulisia natáčania vášho filmu. (Voliteľné)"
                        )
                    ),
                    Arity = new ArgumentArity(Const.BTSPhotoMinCount, Const.BTSPhotoMaxCount),
                    ShardBlueprints = ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint>.Empty
                        .Add(ShardKind.Image,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Behind-the-scenes photograph file"),
                                    (Const.CzechCulture, "Soubor s fotografií ze zákulisí"),
                                    (Const.SlovakCulture, "Súbor s fotografiou zo zákulisia")
                                ),
                                Description = null,
                                Arity = ArgumentArity.ExactlyOne
                            })
                }
            )
    };

    //  MATE PROJECT BLUEPRINT
    public static readonly ProjectBlueprint TemporaryMateProjectBlueprint = new ProjectBlueprint{
        RequiredReviewers = ImmutableArray<string>.Empty,
        ArtifactBlueprints = ImmutableDictionary<string, ProjectArtifactBlueprint>.Empty
            // 3D Model (.blend)
            .Add(Const.BlendBlueprintSlot,
                new ProjectArtifactBlueprint
                {
                    Name = LocalizedString.Create(
                        (Const.InvariantCulture, "3D Model"),
                        (Const.CzechCulture, "3D model"),
                        (Const.SlovakCulture, "3D model")
                    ),
                    Description = null,
                    Arity = ArgumentArity.OneOrMore,
                    ShardBlueprints = ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint>.Empty
                        .Add(ShardKind.Blend,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Blend file"),
                                    (Const.CzechCulture, "Blend soubor"),
                                    (Const.SlovakCulture, "Blend súbor")
                                ),
                                Description = null,
                                Arity = ArgumentArity.ExactlyOne
                            })
                }
            )

            // Rendered images
            .Add(Const.RenderedImageBlueprintSlot,
                new ProjectArtifactBlueprint
                {
                    Name = LocalizedString.Create(
                        (Const.InvariantCulture, "Rendered images"),
                        (Const.CzechCulture, "Vyrenderované obrázky"),
                        (Const.SlovakCulture, "Vyrenderované obrázky")
                    ),
                    Description = null,
                    Arity = ArgumentArity.OneOrMore,
                    ShardBlueprints = ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint>.Empty
                        .Add(ShardKind.Image,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Rendered image"),
                                    (Const.CzechCulture, "Vyrenderovaný obrázek"),
                                    (Const.SlovakCulture, "Vyrenderovaný obrázok")
                                ),
                                Description = null,
                                Arity = ArgumentArity.ExactlyOne
                            })
                }
            )

            // Rendered animations
            .Add(Const.RenderedAnimationBlueprintSlot,
                new ProjectArtifactBlueprint
                {
                    Name = LocalizedString.Create(
                        (Const.InvariantCulture, "Rendered animations"),
                        (Const.CzechCulture, "Vyrenderované animace"),
                        (Const.SlovakCulture, "Vyrenderované animácie")
                    ),
                    Description = null,
                    Arity = ArgumentArity.ZeroOrMore,
                    ShardBlueprints = ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint>.Empty
                        .Add(ShardKind.Video,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Animation render"),
                                    (Const.CzechCulture, "Vyrendrovaná animace"),
                                    (Const.SlovakCulture, "Vyrenderovaná animácia")
                                ),
                                Description = null,
                                Arity = ArgumentArity.ExactlyOne
                            })
                }
            )

            // Textures
            .Add(Const.TextureBlueprintSlot,
                new ProjectArtifactBlueprint
                {
                    Name = LocalizedString.Create(
                        (Const.InvariantCulture, "Textures and other images"),
                        (Const.CzechCulture, "Textury a ostatní obrázky"),
                        (Const.SlovakCulture, "Textúry a ostatné obrázky")
                    ),
                    Description = null,
                    Arity = ArgumentArity.ZeroOrMore,
                    ShardBlueprints = ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint>.Empty
                        .Add(ShardKind.Image,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Texture/other image"),
                                    (Const.CzechCulture, "Textura/ostatní obrázek"),
                                    (Const.SlovakCulture, "Textúra/ostatný obrázok")
                                ),
                                Description = null,
                                Arity = ArgumentArity.ExactlyOne
                            })
                }
            )
    };

    public static ProjectBlueprint GetProjectBlueprintByOrgId(string id)
    {
        switch (id)
        {
            case "mate-fimuni":
                return ProjectBlueprint.TemporaryMateProjectBlueprint;
            case "lemmafimuni":
                return ProjectBlueprint.TemporaryLemmaProjectBlueprint;
            default:
                return ProjectBlueprint.TemporaryProjectBlueprint;
        }
    }
}

public record ProjectArtifactBlueprint
{
    public required LocalizedString Name { get; init; }
    public LocalizedString? Description { get; init; }
    public required ArgumentArity Arity { get; init; }

    public ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint> ShardBlueprints { get; init; }
        = ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint>.Empty;
}

public record ProjectArtifactShardBlueprint
{
    public required LocalizedString Name { get; init; }
    public LocalizedString? Description { get; init; }
    public required ArgumentArity Arity { get; init; }
}