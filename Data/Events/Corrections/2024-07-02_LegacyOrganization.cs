#pragma warning disable 0618

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JasperFx.Core.Reflection;
using Kafe.Data.Aggregates;
using Marten;
using Marten.Events;
using Microsoft.Extensions.Logging;

namespace Kafe.Data.Events.Corrections;

/// <summary>
/// If there are any events prior to the date of the correction, adds a `legacy--org` organization that becomes the
/// owner of all pre-existing project groups and playlists.
/// </summary>
[AutoCorrection("2024-07-02")]
internal class LegacyOrganizationCorrection : IEventCorrection
{
    private readonly ILogger logger;

    public static readonly Hrib LegacyOrganizationId = (Hrib)"legacy--org";
    public static readonly LocalizedString LegacyOrganizationName
        = LocalizedString.CreateInvariant("Legacy Organization");

    public LegacyOrganizationCorrection(ILogger<LegacyOrganizationCorrection> logger)
    {
        this.logger = logger;
    }

    public async Task Apply(IDocumentSession db, CancellationToken ct = default)
    {
        db.Events.KafeStartStream<OrganizationInfo>(LegacyOrganizationId, new OrganizationCreated(
            OrganizationId: LegacyOrganizationId.ToString(),
            CreationMethod: CreationMethod.Correction,
            Name: LegacyOrganizationName
        ));

        var events = await db.Events.QueryAllRawEvents()
            .Where(e =>
                e.EventTypeName == EventMappingExtensions.GetEventTypeName<PlaylistCreated>()
                || e.EventTypeName == EventMappingExtensions.GetEventTypeName<ProjectGroupCreated>())
            .ToListAsync(token: ct);
        foreach (var @event in events)
        {
            if (@event.EventType == typeof(PlaylistCreated))
            {
                db.Events.Append(@event.StreamKey, new PlaylistMovedToOrganization(
                    PlaylistId: @event.StreamKey!,
                    OrganizationId: LegacyOrganizationId.ToString()
                ));
            }
            else if (@event.EventType == typeof(ProjectGroupCreated))
            {
                db.Events.Append(@event.StreamKey, new ProjectGroupMovedToOrganization(
                    ProjectGroupId: @event.StreamKey!,
                    OrganizationId: LegacyOrganizationId.ToString()
                ));
            }
        }
    }
}

