using System.Collections.Generic;

namespace Kafe.Data;

public record DataOptions
{
    public List<string> Languages { get; set; } =
    [
        Const.InvariantCultureCode,
        Const.EnglishCultureName,
        Const.CzechCultureName,
        Const.SlovakCultureName
    ];
}
