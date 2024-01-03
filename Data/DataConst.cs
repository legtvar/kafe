using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Kafe.Data.Aggregates;

namespace Kafe.Data;

public static class DataConst
{
    private static readonly ImmutableDictionary<string, LocalizedString> EntityNames;

    static DataConst()
    {
        EntityNames = ImmutableDictionary.CreateRange(new[] {
            new KeyValuePair<string, LocalizedString>(
                key: typeof(IEntity).Name,
                value: LocalizedString.CreateInvariant("an entity")),
            new KeyValuePair<string, LocalizedString>(
                key: typeof(AccountInfo).Name,
                value: LocalizedString.CreateInvariant("an account")),
            new KeyValuePair<string, LocalizedString>(
                key: typeof(AuthorInfo).Name,
                value: LocalizedString.CreateInvariant("an author")),
            new KeyValuePair<string, LocalizedString>(
                key: typeof(OrganizationInfo).Name,
                value: LocalizedString.CreateInvariant("an organization")),
            new KeyValuePair<string, LocalizedString>(
                key: typeof(RoleInfo).Name,
                value: LocalizedString.CreateInvariant("a role")),
            new KeyValuePair<string, LocalizedString>(
                key: typeof(ProjectGroupInfo).Name,
                value: LocalizedString.CreateInvariant("a project group")),
            new KeyValuePair<string, LocalizedString>(
                key: typeof(ProjectInfo).Name,
                value: LocalizedString.CreateInvariant("a project")),
            new KeyValuePair<string, LocalizedString>(
                key: typeof(ArtifactInfo).Name,
                value: LocalizedString.CreateInvariant("an artifact")),
        });
    }

    public static LocalizedString GetLocalizedName(Type entityType)
    {
        if (EntityNames.TryGetValue(entityType.Name, out var localizedName))
        {
            return localizedName;
        }

        return EntityNames.GetValueOrDefault(typeof(IEntity).Name)
            ?? throw new InvalidOperationException("The localized name for IEntity is missing. This is a bug.");
    }
}
