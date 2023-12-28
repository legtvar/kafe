#pragma warning disable 0618

using System;
using System.Collections.Immutable;
using Marten.Services.Json.Transformations;

namespace Kafe.Data.Events
{
    [Obsolete($"Use {nameof(ProjectGroupEstablished)} instead.")]
    public record ProjectGroupCreated(
        [Hrib] string ProjectGroupId,
        CreationMethod CreationMethod,
        [LocalizedString] ImmutableDictionary<string, string> Name
    );

    [Obsolete($"Use {nameof(PlaylistEstablished)} instead.")]
    public record PlaylistCreated(
        [Hrib] string PlaylistId,
        CreationMethod CreationMethod,
        [LocalizedString] ImmutableDictionary<string, string> Name
    );
}

namespace Kafe.Data.Events.Upcasts
{
    internal class ProjectGroupCreatedUpcaster : EventUpcaster<ProjectGroupCreated, ProjectGroupEstablished>
    {
        public const string LemmaId = "lemmafimuni";

        protected override ProjectGroupEstablished Upcast(ProjectGroupCreated oldEvent)
        {
            return new ProjectGroupEstablished(
                ProjectGroupId: oldEvent.ProjectGroupId,
                CreationMethod: oldEvent.CreationMethod,
                OrganizationId: LemmaId,
                Name: oldEvent.Name);
        }
    }

    internal class PlaylistCreatedUpcaster : EventUpcaster<PlaylistCreated, PlaylistEstablished>
    {
        public const string LemmaId = "lemmafimuni";

        protected override PlaylistEstablished Upcast(PlaylistCreated oldEvent)
        {
            return new PlaylistEstablished(
                PlaylistId: oldEvent.PlaylistId,
                CreationMethod: oldEvent.CreationMethod,
                OrganizationId: LemmaId,
                Name: oldEvent.Name);
        }
    }
}
