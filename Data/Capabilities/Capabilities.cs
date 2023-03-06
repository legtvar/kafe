using System.Text.Json.Serialization;

namespace Kafe.Data.Capabilities;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type", UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization)]
[JsonDerivedType(typeof(AdministratorCapability), nameof(AdministratorCapability))]
[JsonDerivedType(typeof(ReviewerCapability), nameof(ReviewerCapability))]
[JsonDerivedType(typeof(ProjectOwnerCapability), nameof(ProjectOwnerCapability))]
public interface IAccountCapability
{
};

public record AdministratorCapability
    : IAccountCapability;

public record ReviewerCapability(
    string Role
) : IAccountCapability;

public record ProjectOwnerCapability(
    Hrib ProjectId
) : IAccountCapability;
