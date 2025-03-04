namespace Kafe.Core;

[Mod(Name)]
public sealed class CoreMod : IMod
{
    public const string Name = "core";

    public void Configure(ModContext context)
    {
        context.AddArtifactProperty<LocalizedString>(new()
        {
            Name = "localized-string",
            Converter = new LocalizedStringJsonConverter(),
        });
        context.AddArtifactProperty<DateTimeProperty>(new()
        {
            Name = "date-time",
            Converter = new DateTimePropertyJsonConverter(),
        });
        context.AddArtifactProperty<KafeString>(new()
        {
            Name = "string",
            Converter = new KafeStringJsonConverter(),
        });
        context.AddArtifactProperty<NumberProperty>(new()
        {
            Name = "number",
            Converter = new NumberPropertyJsonConverter(),
        });
        context.AddArtifactProperty<ShardReferenceProperty>(new()
        {
            Name = "shard-ref",
            Converter = new ShardReferencePropertyJsonConverter(),
        });
        context.AddArtifactProperty<AuthorReferenceProperty>(new()
        {
            Name = "author-ref",
            DefaultRequirements = [
                new AuthorReferenceNameOrIdRequirement()
            ]
        });
        context.AddRequirement<AuthorReferenceNameOrIdRequirement>(new()
        {
            Name = "author-ref-name-or-id",
            Accessibility = KafeTypeAccessibility.Internal,
            HandlerTypes = [
                typeof(AuthorReferenceNameOrIdRequirementHandler)
            ]
        });
        context.AddShard<BlobShard>(new()
        {
            Name = "blob"
        });
        context.AddShard<ArchiveShard>(new()
        {
            Name = "archive"
        });
    }
}
