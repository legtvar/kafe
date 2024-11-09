using System;
using System.Collections.Concurrent;
using Kafe.Data.Aggregates;
using Microsoft.Extensions.Options;

namespace Kafe.Data.Metadata;

public class EntityMetadataProvider
{
    private readonly IOptions<DataOptions> dataOptions;
    private readonly ConcurrentDictionary<Type, SortableMetadata> sortableMetadataCache
        = new();

    public EntityMetadataProvider(IOptions<DataOptions> dataOptions)
    {
        this.dataOptions = dataOptions;
    }

    public SortableMetadata GetSortableMetadata(Type entityType)
    {
        if (!entityType.IsAssignableTo(typeof(IEntity)))
        {
            throw new ArgumentException($"Type '{entityType.FullName}' is not an IEntity.", nameof(entityType));
        }

        return sortableMetadataCache.GetOrAdd(entityType, e => SortableMetadata.Create(e, dataOptions));
    }
}
