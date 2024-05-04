# Historical Decisions

These decisions were made before we started writing this devlog.
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
