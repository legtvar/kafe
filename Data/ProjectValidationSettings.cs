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

    /// <summary>
    /// Merge <paramref name="left"/> with <paramref name="right"/>, but prefer settings from <paramref name="left"/>.
    /// </summary>
    public static ProjectValidationSettings Merge(ProjectValidationSettings? left, ProjectValidationSettings right)
    {
        if (left is null)
        {
            return right;
        }

        return new ProjectValidationSettings
        {
            MinNameLength = left.MinNameLength ?? right.MinNameLength,
            MaxNameLength = left.MaxNameLength ?? right.MaxNameLength,
            RequiredNameCultures = left.RequiredNameCultures is null || left.RequiredNameCultures.IsEmpty
                ? right.RequiredNameCultures
                : null,
            MinDescriptionLength = left.MinDescriptionLength ?? right.MinDescriptionLength,
            MaxDescriptionLength = left.MaxDescriptionLength ?? right.MaxDescriptionLength,
            RequiredDescriptionCultures =
                left.RequiredDescriptionCultures is null || left.RequiredDescriptionCultures.IsEmpty
                    ? right.RequiredDescriptionCultures
                    : null,
            MinGenreLength = left.MinGenreLength ?? right.MinGenreLength,
            MaxGenreLength = left.MaxGenreLength ?? right.MaxGenreLength,
            RequiredGenreCultures = left.RequiredGenreCultures is null || left.RequiredGenreCultures.IsEmpty
                ? right.RequiredGenreCultures
                : null
        };
    }
}
