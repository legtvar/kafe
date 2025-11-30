using System.Collections.Immutable;

namespace Kafe.Data;

public record ProjectValidationSettings
{
    public static readonly ProjectValidationSettings Default = new ProjectValidationSettings
    {
        MinNameLength = 1,
        MaxNameLength = 42,
        RequiredNameCultures = [Const.InvariantCultureCode, Const.CzechCultureName],
        MinDescriptionLength = 50,
        MaxDescriptionLength = 200,
        RequiredDescriptionCultures = [Const.InvariantCultureCode, Const.CzechCultureName],
        MinGenreLength = 1,
        MaxGenreLength = 32,
        RequiredGenreCultures = [Const.InvariantCultureCode, Const.CzechCultureName],
    };

    public int? MinNameLength { get; init; }

    public int? MaxNameLength { get; init; }

    public ImmutableHashSet<string>? RequiredNameCultures { get; init; }

    public int? MinDescriptionLength { get; init; }

    public int? MaxDescriptionLength { get; init; }

    public ImmutableHashSet<string>? RequiredDescriptionCultures { get; init; }

    public int? MinGenreLength { get; init; }

    public int? MaxGenreLength { get; init; }

    public ImmutableHashSet<string>? RequiredGenreCultures { get; init; }
}
