using System;

namespace Kafe.Core;

[Mod(Name)]
public sealed class CoreMod : IMod
{
    public const string Name = "core";
    
    public void Configure(ModContext context)
    {
        context
            .AddType<LocalizedString>(new() {
                Name = "localized-string",
                Converter = new LocalizedStringJsonConverter(),
                Usage = KafeTypeUsage.ArtifactProperty
            })
            .AddType<KafeDateTime>(new() {
                Name = "date-time",
                Converter = new KafeDateTimeJsonConverter(),
                Usage = KafeTypeUsage.ArtifactProperty
            })
            .AddType<KafeString>(new() {
                Name = "string",
                Converter = new KafeStringJsonConverter(),
                Usage = KafeTypeUsage.ArtifactProperty
            })
            .AddType<KafeNumber>(new() {
                Name = "number",
                Converter = new KafeNumberJsonConverter(),
                Usage = KafeTypeUsage.ArtifactProperty
            })
            .AddType<ShardReference>(new() {
                Name = "shard-ref",
                Converter = new ShardReferenceJsonConverter(),
                Usage = KafeTypeUsage.ArtifactProperty
            })
            .AddType<AuthorReference>(new() {
                Name = "author-ref",
                Usage = KafeTypeUsage.ArtifactProperty,
                DefaultRequirements = [
                    new AuthorReferenceNameOrIdRequirement()
                ]
            })
            .AddRequirement<AuthorReferenceNameOrIdRequirement>(new() {
                Name = "author-ref-name-or-id",
                Accessibility = KafeTypeAccessibility.Internal,
                HandlerTypes = [
                    typeof(AuthorReferenceNameOrIdRequirementHandler)
                ]
            });
    }
}
