using Kafe.Data.Capabilities;
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
    public List<SeedAccount> Accounts { get; set; } = new();

    public List<SeedProjectGroup> ProjectGroups { get; set; } = new();

    public record SeedAccount
    {
        [Required]
        public string EmailAddress { get; set; } = null!;

        public string PreferredCulture { get; set; } = Const.InvariantCultureCode;

        public List<string> Capabilities { get; set; } = new();
    }

    public record SeedProjectGroup
    {
        [Hrib, Required]
        public string Id { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        public string? Deadline { get; set; }
    }
}
