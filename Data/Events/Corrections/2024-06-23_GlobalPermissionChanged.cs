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

namespace Kafe.Data.Events
{
    [Obsolete("Use the *GlobalPermissionChanged for specific entities.")]
    public record GlobalPermissionsChanged(
        [Hrib] string EntityId,
        Permission GlobalPermissions
    );
}

namespace Kafe.Data.Events.Corrections
{
    [AutoCorrection("2024-06-23")]
    internal class GlobalPermissionsChangedCorrection : IEventCorrection
    {
        private readonly IServiceProvider services;
        private readonly ILogger<GlobalPermissionsChangedCorrection> logger;

        public GlobalPermissionsChangedCorrection(
            IServiceProvider services,
            ILogger<GlobalPermissionsChangedCorrection> logger)
        {
            this.services = services;
            this.logger = logger;
        }

        public async Task Apply(IDocumentSession db, CancellationToken ct = default)
        {
            var events = await db.Events.QueryAllRawEvents()
                .Where(e => e.EventTypeName
                    == EventMappingExtensions.GetEventTypeName<GlobalPermissionsChanged>())
                .ToListAsync(token: ct);
            var groups = events.GroupBy(e => e.StreamKey!);
            foreach (var group in groups)
            {
                var streamMetadata = await db.Events.FetchStreamStateAsync(group.Key, ct)
                    ?? throw new ArgumentException($"Stream '{group.Key}' does not seem to exist.");
                if (streamMetadata.AggregateType is null)
                {
                    logger.LogWarning(
                        "Cannot correct GlobalPermissionsChanged events in stream '{StreamKey}' "
                            + "because its could not be determined.",
                        group.Key);
                    continue;
                }

                var lastEvent = group.Last().Data.As<GlobalPermissionsChanged>();
                object? newEvent = null;

                if (streamMetadata.AggregateType == typeof(ProjectGroupInfo))
                {
                    newEvent = new ProjectGroupGlobalPermissionsChanged(
                        lastEvent.EntityId,
                        lastEvent.GlobalPermissions);
                }
                else if (streamMetadata.AggregateType == typeof(ProjectInfo))
                {
                    newEvent = new ProjectGlobalPermissionsChanged(
                        lastEvent.EntityId,
                        lastEvent.GlobalPermissions);
                }
                else if (streamMetadata.AggregateType == typeof(PlaylistInfo))
                {
                    newEvent = new PlaylistGlobalPermissionsChanged(
                        lastEvent.EntityId,
                        lastEvent.GlobalPermissions);
                }
                else if (streamMetadata.AggregateType == typeof(AuthorInfo))
                {
                    newEvent = new AuthorGlobalPermissionsChanged(
                        lastEvent.EntityId,
                        lastEvent.GlobalPermissions);
                }
                else
                {
                    throw new ArgumentException($"The aggregate type of stream '{group.Key}' could not be determined.");
                }

                db.Events.Append(group.Key, newEvent);
            }
        }
    }
}
