using System;
using Kafe.Data;

namespace Kafe.Api.Transfer;

public record ProjectGroupListDto(
    string Id,
    LocalizedString Name,
    LocalizedString? Description,
    DateTimeOffset Deadline,
    bool IsOpen);

public record ProjectGroupDetailDto(
    string Id,
    LocalizedString Name,
    LocalizedString? Description,
    DateTimeOffset Deadline,
    bool IsOpen);
