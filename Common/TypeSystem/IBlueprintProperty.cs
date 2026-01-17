using System.Collections.Generic;

namespace Kafe;

public interface IBlueprintProperty
{
    LocalizedString? Name { get; }

    LocalizedString? Description { get; }

    IReadOnlyList<KafeObject> Requirements { get; }
}
