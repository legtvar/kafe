using System;
using Kafe.Data;

namespace Kafe.Transfer;

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
    bool IsOpen,
    ValidationRules? ValidationRules);
