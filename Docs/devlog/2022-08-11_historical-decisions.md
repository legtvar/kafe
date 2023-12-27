---
title: Historical Decisions
tags: devlog
---

# Historical Decisions

These decisions were made before we started writing this devlog.
(This page bears the date of the first commit to the KAFE repo.)
Nevertheless, they are important to remember.

## Event Sourcing

I decided to try out a pattern I had never used before: [Event Sourcing](https://www.eventstore.com/blog/what-is-event-sourcing).
I heard of it at work.
Its representation of entities as an immutable stream of events have seemed like a good fit for a multimedia archive like KAFE.

I decided to use [Marten](https://martendb.io/) for the event store implementation.
Marten is built around [Postgres](https://www.postgresql.org/) and uses the [Npgsql](https://www.npgsql.org/) library to interact with it.

## Hrib

We use string Human-Readable Identifier Ballast (`Hrib`) for Ids on pretty much everything.
These are essentially YouTube's 11-chars-long Ids but without the checks for swear words.

## No Passwords

We don't want to have the responsibility for saving the user's passwords.
That's why we implemented _temporary_ accounts.
The user is logged in immediately after clicking a one-time link in their email client.
This link is valid only for a short period of time (e.g. a day) and clicking it makes the API set a session cookie.
This session is only temporary and must be refreshed by another email to the same address soon (e.g. after a month)

UPDATE (2023-12-24): Alternatively the user can log in through an external identity provider (e.g. MUNI login).

## `LocalizedString`

Our means of persisting for example project names and descriptions in multiple languages.
All of the language mutations available are saved in an `ImmutableDictonary` and serialized by Marten as a JSON object.
The fallback, invariant mutation is always under "iv".
The others are under their respective two-letter code (e.g. "cs", "en", "sk", etc.).

## Media metadata

The `Media` project integrates our abstractions with libraries that work with media.
We use FFmpeg to get video metadata and convert videos to other formats and resolutions.
FFMpeg must be installed on the machine where KAFE runs.
We **don't** pack our own and access the one available in `Path` through the FFMpegCore wrapper instead.

For images we use _ImageSharp_, which is pretty great.
**However, if you're reading this and want to use KAFE commercially, you should buy ImageSharp's [commercial license](https://sixlabors.com/pricing/) or not use it all.**
