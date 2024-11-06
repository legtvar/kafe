---
title: Architecture Overview
tags: doc
priority: 020
---

# Architecture Overview

KAFE has two main parts: backend and frontend.
The backend is an [ASP.NET Core](https://dotnet.microsoft.com/en-us/apps/aspnet) app written in
[C#](https://learn.microsoft.com/en-us/dotnet/csharp/).
The frontend is a [React](https://react.dev/) app implemented in [TypeScript](https://www.typescriptlang.org/).
The layer between them is defined by [Swagger](https://swagger.io/).

## Repository Structure

In can be a bit daunting to orient yourself around our repository.
Here are some points of interest:

`.gitlab-ci.yml`
~ Configures our build, test, and deployment pipeline.

`docker-compose.*.yml` and `*.Dockerfile`
~ These files define how KAFE is built and run in the Staging and Production environments.

`Web/`
~ The frontend app.

`Kafe.sln`
~ A Visual Studio solution file for the C# portion of KAFE. Open this in VS/VS Code to develop backend.

`Api/`
~ The backend app. Its dependent projects are `Data`, `Media`, and `Common`.

`Data/`
~ A C# class library that `Api` uses to store data in a [Postgres](https://www.postgresql.org/) DB using
[Marten](https://martendb.io/).

`Media/`
~ A C# class library for analyzing video, image, and subtitle files using [FFmpeg](https://ffmpeg.org/)
and [https://github.com/SixLabors/ImageSharp].

`Common/`
~ A C# class library for common types all across the C# projects.

`Tests/`
~ Backend tests built with [Alba](https://jasperfx.github.io/alba/).
Some of these literally simulate HTTP requests to see if the backend really works.
Each tests also spins up its own DB under a temporary schema.

`Migrator/`
~ A C# console app meant to migrate data from legacy systems into KAFE.

`Playground/`
~ A C# console app for messing around.

`Ruv/`
~ A C# library for integration with [Registr uměleckých výstupů](https://msmt.gov.cz/vzdelavani/vysoke-skolstvi/ruv-registr-umeleckych-vystupu). Unfinished. Unused.

`Announcer/`
~ A really simple ASP.NET Core app we use to put up a static "announcement" page.

`.vscode/`
~ VS Code settings and debugging configurations. Feel free to add some but don't remove them.


## Backend

The backend is spread out across the `Api/`, `Data/`, `Media/`, and `Common/` projects.
Over the years the app has grown so it can be quite difficult to figure out what's where and how it works.
Here are some pointers so that you don't stumble entirely in the dark:

### `Kafe.Api`

The main backend app.

`Api/Program.cs`
~ Contains the actual `Main` function of the app.

`Api/Startup.cs`
~ Defines the request pipelines.
Configures services.
Ties everything together.
A bit of mess.

`Api/appsettings.json`
~ Static configuration of the app. Common to all environments.

`Api/appsettings.local.json`
~ Used to fill in missing thing in `appsettings.json`.
This file is modified for each environment.
By default, it can be used to target your local dev machine.
On Production and Staging, we use a different file that overwrites the one in the repo during the deployment pipeline.

`Api/Endpoints/`
~ The HTTP endpoints that make up the API.
Each class defines one endpoint.
(This is not [MVC](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93controller).)

`Api/Transfer/`
~ Types that can be received by the API and are sent back to the client.
Also contains mapping functions from these types to those in `Data/` that are stored in the DB.

### `Kafe.Data`

Data abstraction for both the DB contents and file structure on disk.

`Data/ServiceCollectionExtensions.cs`
~ Contains an extension function (used by the API) that registers and configures Marten.

`Data/Events/`
~ KAFE's [event sourcing model](https://martendb.io/events/).
These event types are KAFE's data model.
Every request that creates or modifies entities has to issue one or more events.
The requests don't make direct changes to the entities themselves.
Those are handled by [projections](https://martendb.io/events/projections/).

`Data/Events/Upcasts/`
~ Since in event sourcing, old and obsolete events stay in the DB, we have
[upcasters](https://martendb.io/events/versioning.html) to convert obsolete events to their newer counterparts.
These upcasters are located here.

`Data/Events/Corrections/`
~ Some DB changes cannot be easily handled by upcasters.
For those, we have our own system of _corrections_.
A correction is simply a thing that is applied to the DB only once and append corrective events.
We store information about which corrections were applied in the `Data/Documents/EventCorrectionInfo.cs` document.

`Data/Aggregates/`
~ Types that stored in the DB as [documents](https://martendb.io/documents/).
Also contains the event projections that create and modify these documents.

`Data/Services/`
~ If we had a [three-tier architecture](https://en.wikipedia.org/wiki/Multitier_architecture), this would be the
business layer. It contains the core of the application logic -- handling projects, project groups, users, etc.

`Data/Projections/EntityPermissionEventProjection.cs`
~ An event projection constructing the `EntityPermissionInfo` and `RoleMemberInfo` documents.
This is the core of KAFE's permission system.


### `Kafe.Media`

Analyzers of media files.

`Media/Services/IMediaService.cs` and `FFmpegCoreService.cs`
~ Analyzes video and subtitle files.

`Media/Services/IImageService.cs` and `ImageSharpService.cs`
~ Analyzes images.


### `Kafe.Common`

Common backend types.

`Common/Hrib.cs`
~ _Human-readable identifier ballast_. 11-letter-long ids used all over KAFE.

`Common/LocalizedString.cs`
~ A string that can have multiple language variantions.
Essentially a dictionary with 2-letter country-codes as keys and string as values.
For example, `iv` is the language-invariant version, `cs` is Czech, and `sk` is Slovak.

`Common/Error.cs`
~ Represents an error in KAFE.
We try to use exceptions only when invalid things happen, like division by zero.
We use these error for everything else, like missing fields and other validation errors.

`Common/Err.cs`
~ An error union type.
Can be either a value of `T` or an `Error`.
Can be unwrapped with `Unwrap` -- this returns `Value` or throws the `Error` as an exception.
