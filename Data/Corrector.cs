using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JasperFx.Core.Reflection;
using Kafe.Data.Documents;
using Marten;
using Marten.Schema;
using Marten.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kafe.Data;

public class Corrector : IInitialData
{
    private readonly IServiceProvider services;
    private readonly ILogger<Corrector> logger;

    public Corrector(
        IServiceProvider services,
        ILogger<Corrector> logger
    )
    {
        this.services = services;
        this.logger = logger;
    }

    public async Task Populate(IDocumentStore store, CancellationToken ct)
    {
        var correctionTypes = typeof(IEventCorrection).Assembly.GetTypes()
            .Where(t => t.GetInterfaces().Contains(typeof(IEventCorrection))
                && t.HasAttribute<AutoCorrectionAttribute>())
            .Select(t =>
            {
                var attribute = t.GetAttribute<AutoCorrectionAttribute>()!;
                return (order: attribute.ImplementedOn, type: t);
            })
            .OrderBy(p => p.order)
            .ToList();

        var db = store.IdentitySession();
        var appliedCorrections = (await db.LoadManyAsync<EventCorrectionInfo>(correctionTypes.Select(t => t.type.Name)))
            .Select(i => i.Id)
            .ToHashSet();

        int lastAppliedCorrectionIndex = -1;
        for (var i = correctionTypes.Count - 1; i >= 0; i--)
        {
            var correctionType = correctionTypes[i];
            if (appliedCorrections.Contains(correctionType.type.Name))
            {
                if (lastAppliedCorrectionIndex < 0)
                {
                    lastAppliedCorrectionIndex = i;
                }
            }
            else if (lastAppliedCorrectionIndex > 0)
            {
                throw new InvalidOperationException(
                    $"Correction '{correctionType.type.Name}' is not applied although a newer "
                    + $"'{correctionTypes[lastAppliedCorrectionIndex].type.Name}' correction already is. "
                    + "Manual intervention is required.");
            }
        }

        if (lastAppliedCorrectionIndex > 0)
        {
            logger.LogInformation(
                "Last correction was '{LastCorrectionId}'.",
                correctionTypes[lastAppliedCorrectionIndex].type.Name);
        }

        for (var i = lastAppliedCorrectionIndex + 1; i < correctionTypes.Count; i++)
        {
            var correctionType = correctionTypes[i].type;
            var correctionInstance = (IEventCorrection)ActivatorUtilities.CreateInstance(services, correctionType);
            logger.LogInformation("Applying correction '{CorrectionId}'.", correctionType.Name);
            await correctionInstance.Apply(db, ct);
            logger.LogDebug("Saving EventCorrectionInfo for '{CorrectionId}'.", correctionType.Name);
            db.Store(new EventCorrectionInfo(
                Id: correctionType.Name,
                AppliedOn: DateTimeOffset.UtcNow,
                AffectedStreams: db.PendingChanges.Streams().Select(s => s.Key!).ToImmutableArray()
            ));
            await db.SaveChangesAsync(ct);
        }
    }
}
