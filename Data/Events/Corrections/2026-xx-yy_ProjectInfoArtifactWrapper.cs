#pragma warning disable 0618

using System;
using System.Collections.Immutable;

namespace Kafe.Data.Events
{

    [Obsolete("ProjectInfo no longer contains author references. Use the artifact property system instead.")]
    public record ProjectAuthorAdded(
        [Hrib] string ProjectId,
        [Hrib] string AuthorId,
        ProjectAuthorKind Kind,
        ImmutableArray<string>? Roles = null
    );

    [Obsolete("ProjectInfo no longer contains author references. Use the artifact property system instead.")]
    public record ProjectAuthorRemoved(
        [Hrib] string ProjectId,
        [Hrib] string AuthorId,
        ProjectAuthorKind? Kind = null,
        ImmutableArray<string>? Roles = null
    );

    [Obsolete("ProjectInfo no longer contains metadata. Use the artifact property system instead.")]
    public record ProjectInfoChanged(
        [Hrib] string ProjectId,
        [LocalizedString] ImmutableDictionary<string, string>? Name = null,
        [LocalizedString] ImmutableDictionary<string, string>? Description = null,
        DateTimeOffset? ReleasedOn = null,
        [LocalizedString] ImmutableDictionary<string, string>? Genre = null
    );

    [Obsolete("ProjectInfo no longer references artifacts through blueprint slots. It wraps a single artifact instead.")]
    public record ProjectArtifactAdded(
        [Hrib] string ProjectId,
        [Hrib] string ArtifactId,
        string? BlueprintSlot
    );

    [Obsolete("ProjectInfo no longer references artifacts through blueprint slots. It wraps a single artifact instead.")]
    public record ProjectArtifactRemoved(
        [Hrib] string ProjectId,
        [Hrib] string ArtifactId
    );
}

namespace Kafe.Data.Aggregates
{
    [Obsolete("ProjectBlueprint was a temporary workaround that has been replaced by the blueprint system.")]
    public record ProjectBlueprint
    {
        public ImmutableArray<string> RequiredReviewers { get; init; } = ImmutableArray<string>.Empty;

        public ImmutableDictionary<string, ProjectArtifactBlueprint> ArtifactBlueprints { get; init; }
            = ImmutableDictionary<string, ProjectArtifactBlueprint>.Empty;

        //  DEFAULT PROJECT BLUEPRINT
        [Obsolete("This temporary blueprint has been replaced by the blueprint system.")]
        public static readonly ProjectBlueprint TemporaryProjectBlueprint = new ProjectBlueprint
        {
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
                                            "English subtitles if the film is in Czech, " +
                                            "or Czech subtitles if the film is in English."
                                        ),
                                        (
                                            Const.CzechCulture,
                                            "Anglické titulky, pokud je film v češtině, " +
                                            "nebo české titulky, pokud je film v angličtině)"
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
                                            "English subtitles if the film is in Czech, " +
                                            "or Czech subtitles if the film is in English."
                                        ),
                                        (
                                            Const.CzechCulture,
                                            "Anglické titulky, pokud je film v češtině, " +
                                            "nebo české titulky, pokud je film v angličtině)"
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
        [Obsolete("This temporary blueprint has been replaced by the blueprint system.")]
        public static readonly ProjectBlueprint TemporaryMateProjectBlueprint = new ProjectBlueprint
        {
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

    [Obsolete("Replaced by the blueprint system.")]
    public record ProjectArtifactBlueprint
    {
        public required LocalizedString Name { get; init; }
        public LocalizedString? Description { get; init; }
        public required ArgumentArity Arity { get; init; }

        public ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint> ShardBlueprints { get; init; }
            = ImmutableDictionary<ShardKind, ProjectArtifactShardBlueprint>.Empty;
    }

    [Obsolete("Replaced by the blueprint system.")]
    public record ProjectArtifactShardBlueprint
    {
        public required LocalizedString Name { get; init; }
        public LocalizedString? Description { get; init; }
        public required ArgumentArity Arity { get; init; }
    }
}
