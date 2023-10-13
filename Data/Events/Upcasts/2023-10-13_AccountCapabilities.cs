using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kafe.Data.Capabilities;
using Marten.Services.Json.Transformations;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Kafe.Data.Capabilities
{
    // NB: Currently broken because the "$type" property would need to be first, which we have no control over
    //     because postgres changes the order.
    //     See: https://github.com/dotnet/runtime/issues/72604
    //     and https://stackoverflow.com/questions/66829778/how-to-control-order-of-fields-in-jsonb-object-agg
    //[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization)]
    //[JsonDerivedType(typeof(AdministratorCapability), nameof(AdministratorCapability))]
    //[JsonDerivedType(typeof(ReviewerCapability), nameof(ReviewerCapability))]
    //[JsonDerivedType(typeof(ProjectOwnerCapability), nameof(ProjectOwnerCapability))]
    [Obsolete($"Capabilities have been superceded by Permissions.")]
    [JsonConverter(typeof(AccountCapabilityConverter))]
    public abstract record AccountCapability
    {
        public const char SubvalueSeparator = ':';

        public static bool TryParse(string value, [NotNullWhen(true)] out AccountCapability? capability)
        {
            // TODO: Rewrite with spans or smth.
            var subvalues = value.Split(SubvalueSeparator);
            capability = subvalues[0] switch
            {
                nameof(Administration) => new Administration(),
                nameof(ProjectReview) => new ProjectReview(subvalues[1]),
                nameof(ProjectOwnership) => new ProjectOwnership(subvalues[1]),
                nameof(AuthorManagement) => new AuthorManagement(subvalues[1]),
                nameof(OrganizeFestival) => new OrganizeFestival(),
                _ => null
            };
            return capability != null;
        }

        public static string Serialize(AccountCapability capability)
        {
            return capability switch
            {
                Administration => nameof(Administration),
                ProjectReview r => $"{nameof(ProjectReview)}:{r.Role}",
                ProjectOwnership o => $"{nameof(ProjectOwnership)}:{o.ProjectId}",
                AuthorManagement a => $"{nameof(AuthorManagement)}:{a.AuthorId}",
                OrganizeFestival => nameof(OrganizeFestival),
                _ => throw new NotImplementedException($"Serialization of '{capability.GetType()}' is not implemented.")
            };
        }

        public static bool IsValidCapabilitySubvalue(string value)
        {
            return value.All(c => c != SubvalueSeparator && c >= ' ' && c <= '~');
        }

        [return: NotNullIfNotNull(nameof(capability))]
        public static implicit operator string?(AccountCapability? capability)
        {
            return capability is null ? null : Serialize(capability);
        }

        [return: NotNullIfNotNull(nameof(capabilityString))]
        public static explicit operator AccountCapability?(string? capabilityString)
        {
            return capabilityString is null
                ? null
                : TryParse(capabilityString, out var capability)
                    ? capability
                    : throw new ArgumentException(
                        $"Could not parse as {nameof(AccountCapability)}.",
                        nameof(capabilityString));
        }

    }

    [Obsolete($"Capabilities have been superceded by Permissions.")]
    public record Administration : AccountCapability;

    [Obsolete($"Capabilities have been superceded by Permissions.")]
    public record ProjectReview : AccountCapability
    {
        public string Role { get; init; }

        public ProjectReview(string role)
        {
            // TODO: Add a validation attribute / source generator for this.
            Role = IsValidCapabilitySubvalue(role)
                ? role
                : throw new ArgumentException($"Capability subvalues can contain only ASCII letters " +
                    $"and digits except '{SubvalueSeparator}'.", nameof(role));
        }
    }

    [Obsolete($"Capabilities have been superceded by Permissions.")]
    public record ProjectOwnership : AccountCapability
    {
        public Hrib ProjectId { get; init; }

        public ProjectOwnership(Hrib projectId)
        {
            ProjectId = IsValidCapabilitySubvalue(projectId)
                ? projectId
                : throw new ArgumentException($"Capability subvalues can contain only ASCII letters " +
                    $"and digits except '{SubvalueSeparator}'.", nameof(projectId));
        }
    }

    [Obsolete($"Capabilities have been superceded by Permissions.")]
    public record AuthorManagement : AccountCapability
    {
        public Hrib AuthorId { get; init; }

        public AuthorManagement(Hrib authorId)
        {
            AuthorId = IsValidCapabilitySubvalue(authorId)
                ? authorId
                : throw new ArgumentException($"Capability subvalues can contain only ASCII letters " +
                    $"and digits except '{SubvalueSeparator}'.", nameof(authorId));
        }
    }

    [Obsolete($"Capabilities have been superceded by Permissions.")]
    public record OrganizeFestival : AccountCapability;

    [Obsolete($"Capabilities have been superceded by Permissions.")]
    public class AccountCapabilityConverter : JsonConverter<AccountCapability>
    {
        public override AccountCapability? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (value is null)
            {
                return null;
            }

            if (!AccountCapability.TryParse(value, out var capability))
            {
                throw new JsonException($"An {nameof(AccountCapability)} could not be deserialized.");
            }

            return capability;
        }

        public override void Write(Utf8JsonWriter writer, AccountCapability value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}

namespace Kafe.Data.Events
{
    [Obsolete($"Capabilities have been superceded by Permissions. Use {nameof(AccountPermissionSet)} instead.")]
    public record AccountCapabilityAdded(
        [Hrib] string AccountId,
        string Capability
    );

    [Obsolete($"Capabilities have been superceded by Permissions. Use {nameof(AccountPermissionSet)} instead.")]
    public record AccountCapabilityRemoved(
        [Hrib] string AccountId,
        string Capability
    );
}

#pragma warning disable 0618
namespace Kafe.Data.Events.Upcasts
{
    internal class AccountCapabilityAddedUpcaster : EventUpcaster<AccountCapabilityAdded, AccountPermissionSet>
    {
        public const string Fffimu23Id = "0JY2-TPhOC7";

        protected override AccountPermissionSet Upcast(AccountCapabilityAdded oldEvent)
        {
            var capability = (AccountCapability)oldEvent.Capability;
            return capability switch
            {
                Administration => new AccountPermissionSet(
                    AccountId: oldEvent.AccountId,
                    EntityId: Hrib.System,
                    Permission: Permission.All),
                ProjectReview => new AccountPermissionSet(
                    AccountId: oldEvent.AccountId,
                    EntityId: Fffimu23Id,
                    Permission: Permission.All),
                ProjectOwnership p => new AccountPermissionSet(
                    AccountId: oldEvent.AccountId,
                    EntityId: p.ProjectId,
                    Permission: Permission.All),
                AuthorManagement a => new AccountPermissionSet(
                    AccountId: oldEvent.AccountId,
                    EntityId: a.AuthorId,
                    Permission: Permission.All),
                OrganizeFestival => new AccountPermissionSet(
                    AccountId: oldEvent.AccountId,
                    EntityId: Fffimu23Id,
                    Permission: Permission.All),
                _ => throw new NotSupportedException()
            };
        }
    }

    internal class AccountCapabilityRemovedUpcaster : EventUpcaster<AccountCapabilityRemoved, AccountPermissionUnset>
    {
        public const string Fffimu23Id = "0JY2-TPhOC7";

        protected override AccountPermissionUnset Upcast(AccountCapabilityRemoved oldEvent)
        {
            var capability = (AccountCapability)oldEvent.Capability;
            return capability switch
            {
                Administration => new AccountPermissionUnset(
                    AccountId: oldEvent.AccountId,
                    EntityId: Hrib.System),
                ProjectReview => new AccountPermissionUnset(
                    AccountId: oldEvent.AccountId,
                    EntityId: Fffimu23Id),
                ProjectOwnership p => new AccountPermissionUnset(
                    AccountId: oldEvent.AccountId,
                    EntityId: p.ProjectId),
                AuthorManagement a => new AccountPermissionUnset(
                    AccountId: oldEvent.AccountId,
                    EntityId: a.AuthorId),
                OrganizeFestival => new AccountPermissionUnset(
                    AccountId: oldEvent.AccountId,
                    EntityId: Fffimu23Id),
                _ => throw new NotSupportedException()
            };
        }
    }
}
