using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Diagnostics;

public class RequirementValidator(
    IServiceProvider serviceProvider,
    KafeTypeRegistry typeRegistry
)
{
    public async Task<ArtifactValidationReport> ValidateArtifact(
        IArtifact artifact,
        IBlueprint blueprint,
        CancellationToken ct = default
    )
    {
        var validationStarted = DateTimeOffset.UtcNow;
        var diagnosticsBuilder = ImmutableArray.CreateBuilder<Diagnostic>();

        // TODO: Parallelize
        foreach (var property in blueprint.Properties)
        {
            KafeObject? target = artifact.Properties.ContainsKey(property.Key)
                ? artifact.Properties[property.Key]
                : null;
            foreach (var requirement in property.Value.Requirements)
            {
                diagnosticsBuilder.AddRange(await ValidateRequirement((IRequirement)requirement.Value, target, ct));
            }
        }

        // TODO: handle default requirements for scalar types

        if (!blueprint.AllowAdditionalProperties)
        {
            // TODO
        }

        return new ArtifactValidationReport(artifact.Id, blueprint.Id, validationStarted, diagnosticsBuilder.ToImmutable());
    }

    public async Task<ImmutableArray<Diagnostic>> ValidateRequirement(
        IRequirement requirement,
        KafeObject? target,
        CancellationToken ct = default
    )
    {
        var context = new RequirementContext<IRequirement>(requirement, target, serviceProvider, ct);
        var requirementType = typeRegistry.RequireType(requirement.GetType());
        var requirementMetadata = typeRegistry.RequireMetadata(requirementType)
            .RequireExtension<RequirementTypeMetadata>();
        var handlers = requirementMetadata.HandlerTypes
            .Select(ht => (IRequirementHandler)ActivatorUtilities.CreateInstance(serviceProvider, ht))
            .ToImmutableArray();
        foreach (var handler in handlers)
        {
            if (handler.CanHandle(requirement))
            {
                await handler.Handle(context);
            }
        }

        return [.. context.Diagnostics];
    }
}
