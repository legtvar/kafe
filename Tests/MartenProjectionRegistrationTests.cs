using System;
using System.Linq;
using JasperFx.Events;
using JasperFx.Events.Projections;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using Xunit;

namespace Kafe.Tests;

public class MartenProjectionRegistrationTests
{
    [Fact]
    public void AggregateFactories_ShouldNotBeRegisteredAsEventHandlers()
    {
        var options = new StoreOptions();
        options.Connection("host=localhost;database=unused;username=unused;password=unused");
        options.Events.StreamIdentity = StreamIdentity.AsString;
        options.Projections.Add<AuthorInfoProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<ArtifactInfoProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<OrganizationInfoProjection>(ProjectionLifecycle.Inline);

        using var store = new DocumentStore(options);
        var eventTypes = store.Options.Projections.All
            .OfType<EventFilterable>()
            .SelectMany(p => p.IncludedEventTypes)
            .ToArray();

        // This is the same registration performed by the projections CLI when it describes the store.
        store.Options.Events.AddEventTypes(eventTypes);

        Assert.DoesNotContain(typeof(string), eventTypes);
        Assert.DoesNotContain(typeof(LocalizedString), eventTypes);
        Assert.Contains(typeof(AuthorCreated), eventTypes);
        Assert.Contains(typeof(ArtifactCreated), eventTypes);
        Assert.Contains(typeof(OrganizationCreated), eventTypes);
    }
}
