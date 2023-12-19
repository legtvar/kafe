using System;
using Marten.Services.Json.Transformations;

#pragma warning disable 0618
namespace Kafe.Data.Events
{
    [Obsolete($"This event has been superceded by another.")]
    public record PlaylistVideoAdded(
        [Hrib] string PlaylistId,
        [Hrib] string VideoId
    );

    [Obsolete($"This event has been superceded by another.")]
    public record PlaylistVideoRemoved(
        [Hrib] string PlaylistId,
        [Hrib] string VideoId
    );

    [Obsolete($"This event has been superceded by another.")]
    public record PlaylistVideoOrderChanged(
        [Hrib] string PlaylistId,
        [Hrib] string VideoId,
        int NewIndex
    );
}

namespace Kafe.Data.Events.Upcasts
{
    internal class PlaylistVideoAddedUpcaster : EventUpcaster<PlaylistVideoAdded, PlaylistEntryAppended>
    {
        protected override PlaylistEntryAppended Upcast(PlaylistVideoAdded oldEvent)
        {
            return new PlaylistEntryAppended(
                PlaylistId: oldEvent.PlaylistId,
                ArtifactId: oldEvent.VideoId);
        }
    }

    internal class PlaylistVideoRemovedUpcaster : EventUpcaster<PlaylistVideoRemoved, PlaylistEntryRemovedFirst>
    {
        protected override PlaylistEntryRemovedFirst Upcast(PlaylistVideoRemoved oldEvent)
        {
            return new PlaylistEntryRemovedFirst(
                PlaylistId: oldEvent.PlaylistId,
                ArtifactId: oldEvent.VideoId);
        }
    }
}
