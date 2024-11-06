---
title: Fake Aggregates Removed
---

# Fake Aggregates Removed

Turns out two Marten aggregate types cannot share an event type.
Well, actually it's fine as long as the event always updates all aggregates whose projections use it.
This is not that case of my `GlobalPermissionsChanged` event, which I used to set default permissions for any entity.
That means that all of the entity projections (e.g., `ProjectInfoProjection`, `AuthorProjection`, `PlaylistProjection`, etc.) had an `Apply` method for this event type.
Little did I know, that every time I'd use `GlobalPermissionsChanged` on, for example a project, Marten would also create a "fake" author, project group, and playlist with the same HRIB.

The fix for this turned out to be a little more complicated than I anticipated.
I had to replace the already existing `GlobalPermissionChanged` events with their more type specific counterparts (e.g., `ProjectGlobalPermissionsChanged`).
I couldn't just use a Marten [upcaster](https://martendb.io/events/versioning.html), as those are 1-to-1.
So I added a _correction_ subsystem.

An _event correction_ is essentially a callback invoked at the start of the application.
It finds all types in the `Kafe.Data` assembly that implement `IEventCorrection` and have the `AutoCorrectionAttribute`.
The attribute an `ImplementedOn` property, giving the order in which event corrections are applied.
KAFE checks which correction had been applied the last, and if there are newly implemented corrections runs those as well.

This way I fixed the `GlobalPermissionsChanged` issue by removing the type altogether from all of the projections, and then implementing a correction that appends new aggregate-specific events to the affected streams.

What a bug...
