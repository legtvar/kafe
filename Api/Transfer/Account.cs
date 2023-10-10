﻿using System.Collections.Immutable;

namespace Kafe.Api.Transfer;

/// <summary>
/// A detail of a user account.
/// </summary>
/// <param name="Id">The id of the account.</param>
/// <param name="Name">The name of the user. Null if the account is temporary.</param>
/// <param name="Uco">The uco of the user. Null if the account is temporary.</param>
/// <param name="EmailAddress">The email address of the user.</param>
/// <param name="PreferredCulture">The preferred culture of the user.</param>
/// <param name="Capabilities">The capabilities this user has been granted.</param>
public record AccountDetailDto(
    Hrib Id,
    string? Name,
    string? Uco,
    string EmailAddress,
    string PreferredCulture,
    ImmutableHashSet<string> Capabilities
);

public record AccountListDto(
    Hrib Id,
    string EmailAddress,
    string PreferredCulture,
    ImmutableHashSet<string> Capabilities
);
