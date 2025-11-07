using System;
using Marten.Events.CodeGeneration;
using Marten.Metadata;
using Newtonsoft.Json;

namespace Kafe.Data.Documents;

public record LoginTicketInfo(
    Guid Id,
    string EmailAddress,
    string PreferredCulture,
    [Hrib] string? AccountId,
    [Hrib] string? InviteId,
    DateTimeOffset CreatedAt
) : ISoftDeleted
{
    // NB: these properties are set based on the mt_deleted and mt_deleted_at columns
    // see https://github.com/JasperFx/marten/issues/2924
    [JsonIgnore]
    public bool Deleted { get; set; }

    [JsonIgnore]
    public DateTimeOffset? DeletedAt { get; set; }
}
