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
                        (Const.CzechCulture, "Film")
                    ),
                    Description = null,
                    Arity = ArgumentArity.ExactlyOne,

                    ShardBlueprints = ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint>.Empty
                        .Add(ShardKind.Video,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Film file"),
                                    (Const.CzechCulture, "Soubor s filmem")
                                ),
                                Description = null,
                                Arity = ArgumentArity.ExactlyOne
                            })
                        .Add(ShardKind.Subtitles,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Subtitles"),
                                    (Const.CzechCulture, "Titulky")
                                ),
                                Description = LocalizedString.Create(
                                    (
                                        Const.InvariantCulture,
                                        "English subtitles if the film is in Czech or Slovak, " +
                                        "or Czech/Slovak subtitles if the film is in English."
                                    ),
                                    (
                                        Const.CzechCulture,
                                        "Anglické titulky, pokud je film v češtině nebo slovenštine, " +
                                        "nebo české/slovenské titulky, pokud je film v angličtině."
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
                        (Const.CzechCulture, "Videoanotace")
                    ),
                    Description = null,
                    Arity = ArgumentArity.ZeroOrOne,

                    ShardBlueprints = ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint>.Empty
                        .Add(ShardKind.Video,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Video-annotation file"),
                                    (Const.CzechCulture, "Soubor s videoanotací")
                                ),
                                Description = null,
                                Arity = ArgumentArity.ExactlyOne
                            })
                        .Add(ShardKind.Subtitles,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Subtitles"),
                                    (Const.CzechCulture, "Titulky")
                                ),
                                Description = LocalizedString.Create(
                                    (
                                        Const.InvariantCulture,
                                        "English subtitles if the film is in Czech or Slovak, " +
                                        "or Czech/Slovak subtitles if the film is in English."
                                    ),
                                    (
                                        Const.CzechCulture,
                                        "Anglické titulky, pokud je film v češtině nebo slovenštine, " +
                                        "nebo české/slovenské titulky, pokud je film v angličtině."
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
                        (Const.InvariantCulture, "Cover photo"),
                        (Const.CzechCulture, "Titulní fotografie")
                    ),
                    Description = null,
                    Arity = new ArgumentArity(Const.CoverPhotoMinCount, Const.CoverPhotoMaxCount),
                    ShardBlueprints = ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint>.Empty
                        .Add(ShardKind.Image,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Cover photo file"),
                                    (Const.CzechCulture, "Soubor s titulní fotografií")
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
                        (Const.CzechCulture, "3D model")
                    ),
                    Description = null,
                    Arity = ArgumentArity.OneOrMore,
                    ShardBlueprints = ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint>.Empty
                        .Add(ShardKind.Blend,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Blend file"),
                                    (Const.CzechCulture, "Blend soubor")
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
                        (Const.CzechCulture, "Vyrenderované obrázky")
                    ),
                    Description = null,
                    Arity = ArgumentArity.OneOrMore,
                    ShardBlueprints = ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint>.Empty
                        .Add(ShardKind.Image,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Rendered image"),
                                    (Const.CzechCulture, "Vyrenderovaný obrázek")
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
                        (Const.CzechCulture, "Vyrenderované animace")
                    ),
                    Description = null,
                    Arity = ArgumentArity.ZeroOrMore,
                    ShardBlueprints = ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint>.Empty
                        .Add(ShardKind.Video,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Animation render"),
                                    (Const.CzechCulture, "Vyrendrovaná animace")
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
                        (Const.CzechCulture, "Textury a ostatní obrázky")
                    ),
                    Description = null,
                    Arity = ArgumentArity.ZeroOrMore,
                    ShardBlueprints = ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint>.Empty
                        .Add(ShardKind.Image,
                            new ProjectArtifactShardBlueprint
                            {
                                Name = LocalizedString.Create(
                                    (Const.InvariantCulture, "Texture/other image"),
                                    (Const.CzechCulture, "Textura/ostatní obrázek")
                                ),
                                Description = null,
                                Arity = ArgumentArity.ExactlyOne
                            })
                }
            )
    };

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