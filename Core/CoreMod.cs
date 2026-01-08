using System;
using Kafe.Core.Requirements;

namespace Kafe.Core;

public sealed class CoreMod : IMod
{
    public static string Moniker => "core";

    public void Configure(ModContext c)
    {
        AddScalars(c);
        AddShards(c);
        AddRequirements(c);
        AddDiagnostics(c);
    }

    private static void AddScalars(ModContext c)
    {
        c.AddScalar<LocalizedString>(new ScalarRegistrationOptions
        {
            Title = LocalizedString.Create(
                (Const.InvariantCulture, "Localized string"),
                (Const.CzechCulture, "Lokalizovaný řetězec")
            ),
            Converter = new LocalizedStringJsonConverter(),
        });
        c.AddScalar(typeof(DateTimeOffset?), new ScalarRegistrationOptions
        {
            Moniker = "date-time",
            Title = LocalizedString.Create(
                (Const.InvariantCulture, "Date-time"),
                (Const.CzechCulture, "Datum/čas")
            )
        });
        c.AddScalar(typeof(string), new ScalarRegistrationOptions
        {
            Moniker = "string",
            Title = LocalizedString.Create(
                (Const.InvariantCulture, "String"),
                (Const.CzechCulture, "Řetězec")
            )
        });
        c.AddScalar(typeof(decimal?), new ScalarRegistrationOptions
        {
            Moniker = "number",
            Title = LocalizedString.Create(
                (Const.InvariantCulture, "Number"),
                (Const.CzechCulture, "Číslo")
            )
        });
        c.AddScalar<ShardReference>(new ScalarRegistrationOptions
        {
            Converter = new ShardReferencePropertyJsonConverter(),
        });
        c.AddScalar<AuthorReference>(new ScalarRegistrationOptions
        {
            DefaultRequirements = [
                new AuthorReferenceNameOrIdRequirement()
            ]
        });
    }

    private static void AddShards(ModContext c)
    {
        c.AddShardPayload<BlobShard>();
        c.AddShardPayload<ArchiveShard>();
    }

    private static void AddRequirements(ModContext c)
    {
        c.AddRequirement<AuthorReferenceNameOrIdRequirement>(new RequirementRegistrationOptions
        {
            HandlerTypes = [typeof(AuthorReferenceNameOrIdRequirementHandler)]
        });
        c.AddRequirement<StringLengthRequirement>(new RequirementRegistrationOptions
        {
            HandlerTypes = [typeof(StringLengthRequirementHandler)]
        });
        c.AddRequirement<ShardFileLengthRequirement>(new RequirementRegistrationOptions
        {
            HandlerTypes = [typeof(ShardFileLengthRequirementHandler)]
        });
        // TODO: Implement handlers
        c.AddRequirement<AllRequirement>();
        c.AddRequirement<ArrayLengthRequirement>();
        c.AddRequirement<ShardMetadataTypeRequirement>();
    }

    private static void AddDiagnostics(ModContext c)
    {
        c.AddDiagnosticFromAssembly(typeof(CoreMod).Assembly);
    }
}
