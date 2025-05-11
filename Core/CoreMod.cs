using Kafe.Core.Requirements;

namespace Kafe.Core;

public sealed class CoreMod : IMod
{
    public static string Moniker { get; } = "core";

    public void Configure(ModContext c)
    {
        AddArtifactProperties(c);
        AddShards(c);
        AddRequirements(c);
        AddDiagnostics(c);
    }

    private static void AddArtifactProperties(ModContext c)
    {
        c.AddArtifactProperty<LocalizedString>(new()
        {
            Moniker = "localized-string",
            Converter = new LocalizedStringJsonConverter(),
        });
        c.AddArtifactProperty<DateTimeProperty>(new()
        {
            Moniker = "date-time",
            Converter = new DateTimePropertyJsonConverter(),
        });
        c.AddArtifactProperty<KafeString>(new()
        {
            Moniker = "string",
            Converter = new KafeStringJsonConverter(),
        });
        c.AddArtifactProperty<NumberProperty>(new()
        {
            Moniker = "number",
            Converter = new NumberPropertyJsonConverter(),
        });
        c.AddArtifactProperty<ShardReferenceProperty>(new()
        {
            Moniker = "shard-ref",
            Converter = new ShardReferencePropertyJsonConverter(),
        });
        c.AddArtifactProperty<AuthorReferenceProperty>(new()
        {
            Moniker = "author-ref",
            DefaultRequirements = [
                new AuthorReferenceNameOrIdRequirement()
            ]
        });
    }

    private static void AddShards(ModContext c)
    {
        c.AddShard<BlobShard>(new()
        {
            Moniker = "blob"
        });
        c.AddShard<ArchiveShard>(new()
        {
            Moniker = "archive"
        });
    }

    private static void AddRequirements(ModContext c)
    {
        c.AddRequirement<AuthorReferenceNameOrIdRequirement>(new()
        {
            HandlerTypes = [typeof(AuthorReferenceNameOrIdRequirementHandler)]
        });
        c.AddRequirement<StringLengthRequirement>(new()
        {
            HandlerTypes = [typeof(StringLengthRequirementHandler)]
        });
        c.AddRequirement<ShardFileLengthRequirement>(new()
        {
            HandlerTypes = [typeof(ShardFileLengthRequirementHandler)]
        });
    }

    private static void AddDiagnostics(ModContext c)
    {
        c.AddDiagnosticFromAssembly(typeof(CoreMod).Assembly);
    }
}
