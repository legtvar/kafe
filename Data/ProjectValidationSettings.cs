using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Kafe.Data;

public record ProjectValidationSettings
{
    public static readonly ProjectValidationSettings Default = new ProjectValidationSettings
    {
        MinNameLength = 1,
        MaxNameLength = 42,
        RequiredNameCultures = [Const.EnglishCultureName, Const.CzechOrSlovakPseudoCultureName],
        MinDescriptionLength = 50,
        MaxDescriptionLength = 300,
        RequiredDescriptionCultures = [Const.EnglishCultureName, Const.CzechOrSlovakPseudoCultureName],
        MinGenreLength = 1,
        MaxGenreLength = 134,
        RequiredGenreCultures = [],
    };

    public static readonly ProjectValidationSettings MateValidationSettings = new ProjectValidationSettings
    {
        MinNameLength = 1,
        MaxNameLength = 42,
        RequiredNameCultures = [Const.InvariantCultureCode],
        MinDescriptionLength = 0,
        MaxDescriptionLength = 10000,
        RequiredDescriptionCultures = [],
        MinGenreLength = 0,
        MaxGenreLength = 0,
        RequiredGenreCultures = [],
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
            RequiredNameCultures = left.RequiredNameCultures is null
                ? right.RequiredNameCultures
                : left.RequiredNameCultures,
            MinDescriptionLength = left.MinDescriptionLength ?? right.MinDescriptionLength,
            MaxDescriptionLength = left.MaxDescriptionLength ?? right.MaxDescriptionLength,
            RequiredDescriptionCultures =
                left.RequiredDescriptionCultures is null
                    ? right.RequiredDescriptionCultures
                    : left.RequiredDescriptionCultures,
            MinGenreLength = left.MinGenreLength ?? right.MinGenreLength,
            MaxGenreLength = left.MaxGenreLength ?? right.MaxGenreLength,
            RequiredGenreCultures = left.RequiredGenreCultures is null
                ? right.RequiredGenreCultures
                : left.RequiredGenreCultures
        };
    }

    public virtual bool Equals(ProjectValidationSettings? other)
    {
        if (other is null)
        {
            return false;
        }

        return MinNameLength == other.MinNameLength
            && MaxNameLength == other.MaxNameLength
            && SetEquals(RequiredNameCultures, other.RequiredNameCultures)
            && MinDescriptionLength == other.MinDescriptionLength
            && MaxDescriptionLength == other.MaxDescriptionLength
            && SetEquals(RequiredDescriptionCultures, other.RequiredDescriptionCultures)
            && MinGenreLength == other.MinGenreLength
            && MaxGenreLength == other.MaxGenreLength
            && SetEquals(RequiredGenreCultures, other.RequiredGenreCultures);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            MinNameLength,
            MaxNameLength,
            MinDescriptionLength,
            MaxDescriptionLength,
            MinGenreLength,
            MaxDescriptionLength
        )
        ^ HashCode.Combine(
            RequiredNameCultures?.Count,
            RequiredDescriptionCultures?.Count,
            RequiredGenreCultures?.Count
        );
    }

    private static bool SetEquals<T>(ISet<T>? lhs, ISet<T>? rhs)
    {
        if (lhs is null && rhs is null)
        {
            return true;
        }

        if (lhs is null || rhs is null)
        {
            return false;
        }

        return lhs.SetEquals(rhs);
    }
}
