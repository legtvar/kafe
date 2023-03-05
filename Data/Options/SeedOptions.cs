using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data.Options;

public record SeedOptions
{
    public List<SeedAccount> Accounts { get; init; } = new();

    public record SeedAccount
    {
        [Required]
        public string EmailAddress { get; init; } = null!;

        public string PreferredCulture { get; init; } = Const.InvariantCultureCode;

        public ImmutableArray<string> Capabilities { get; init; }
            = ImmutableArray<string>.Empty;
    }
}
