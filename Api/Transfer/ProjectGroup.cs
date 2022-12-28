using Kafe.Data;

namespace Kafe.Transfer;

public record ProjectGroupListDto(
    string Id,
    string? Name,
    string? Description,
    string? EnglishName,
    string? EnglishDescription,
    DateTimeOffset Deadline,
    bool IsOpen);

public record ProjectGroupDetailDto(
    string Id,
    string? Name,
    string? Description,
    string? EnglishName,
    string? EnglishDescription,
    DateTimeOffset Deadline,
    bool IsOpen,
    ValidationRules? ValidationRules);
