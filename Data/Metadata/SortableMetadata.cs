using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JasperFx.Core.Reflection;
using Kafe.Data.Aggregates;
using Microsoft.Extensions.Options;

namespace Kafe.Data.Metadata;

public sealed record SortableMetadata
{
    public ImmutableDictionary<string, string> SortExpressions { get; init; }
        = ImmutableDictionary<string, string>.Empty;

    public static SortableMetadata Create(Type entityType, IOptions<DataOptions> dataOptions)
    {
        if (!entityType.IsAssignableTo(typeof(IEntity)))
        {
            throw new ArgumentException($"Type '{entityType.FullName}' is not an IEntity.", nameof(entityType));
        }

        var sortableProperties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.IsDefined(typeof(SortableAttribute)))
            .ToImmutableArray();

        var sortExpressionsBuilder = ImmutableDictionary.CreateBuilder<string, string>();
        foreach (var prop in sortableProperties)
        {
            var name = char.ToLower(prop.Name[0]) + prop.Name[1..];
            if (prop.PropertyType.IsString()
                || prop.PropertyType.IsNumeric()
                || prop.PropertyType.IsBoolean()
                || prop.PropertyType.IsDateTime()
                || prop.PropertyType == typeof(DateTimeOffset))
            {
                sortExpressionsBuilder.Add(name, $"d.data ->> '{prop.Name}'");
            }
            else if (prop.IsDefined(typeof(LocalizedStringAttribute))
                || prop.PropertyType == typeof(LocalizedString))
            {
                if (prop.PropertyType != typeof(LocalizedString)
                    && prop.PropertyType != typeof(ImmutableDictionary<string, string>))
                {
                    throw new InvalidOperationException($"Sortable property '{prop.Name}' on type "
                    + $"'{entityType.Name}' has the LocalizedString attribute but is not of type LocalizedString or "
                    + "ImmutableDictionary<string, string>.");
                }

                sortExpressionsBuilder.Add(name, $"d.data -> '{prop.Name}' ->> 'iv'");
                sortExpressionsBuilder.Add($"{name}.iv", $"d.data -> '{prop.Name}' ->> 'iv'");
                foreach (var language in dataOptions.Value.Languages.Except([Const.InvariantCultureCode]))
                {
                    sortExpressionsBuilder.Add(
                        $"{name}.{language}",
                        $"COALESCE(d.data -> '{prop.Name}' ->> '{language}', d.data -> '{prop.Name}' ->> 'iv')"
                    );
                }
            }
        }

        return new()
        {
            SortExpressions = sortExpressionsBuilder.ToImmutable()
        };
    }
}
