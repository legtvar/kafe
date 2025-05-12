---
title: Getting Started
tags: doc
priority: 010
---

# Getting Started

1. Install the prerequisites below.
1. Clone the repository.
2. Optionally [set up a VM](./vm-setup) or [Docker](https://www.docker.com/).
3. Install KAFE using Docker compose or run it locally through `pnpm` or `dotnet` CLI.


## Prerequisites

**Back end (API)**

* [.NET SDK 9.0 or newer](https://dotnet.microsoft.com/en-us/download).
* [PostgreSQL 17 or newer](https://www.postgresql.org/)
* [FFmpeg](https://ffmpeg.org/) in your [PATH](https://en.wikipedia.org/wiki/PATH_(variable))

**Front end (Web)**

* [Node.js v22](https://nodejs.org/en/download/package-manager).
* [pnpm](https://pnpm.io/).


## Configuration

**Back end (API)**

The API is configured using the `Api/appsettings.local.json`.
By default:

- The API runs at `https://localhost:44369`.
- It saves artifacts to `C:/dev/fi/kafe-archive`.
- It generates lower-resolution videos in `C:/dev/fi/kafe-generated`.
- It doesn't send emails, it just logs them into the terminal.
- It creates an admin account with email `kafe@example.com`.
- It expects an OpenIdConnect server at `https://localhost:44370`.

**Front end (Web)**

The front end is configured with `Web/.env` and `Web/src/api/API.ts`.
By default:

- It expects the api to run locally at `https://localhost:44369`.


## Running locally

You can run KAFE locally either directly on your machine or through Docker.
If you run into issues, check the instructions below and [Troubleshooting](./troubleshooting.md).

### Without Docker

These instructions explain, how to run KAFE on your local machine, **not in a Docker container**.
This has the advantage of being easier to debug.
However, it takes longer to set up and may conflict with your local configs if you have other projects using .NET, Node.js, or Postgres.

**Back end (API)**

1. Make sure Postgres is running on port **5432**.
   If it's running on a different port, update connection strings in `Api/appsettings.local.json`.
2. Restore NuGet packages: `dotnet restore`
3. If you haven't already, create a self-signed dev certificate: `dotnet dev-certs https --trust`
4. While in the `Api/` dir, to run the API: `dotnet run`
5. Run the front end or use Postman.

**Front end (Web)**

1. Go to the `Web` directory.
2. Ensure `pnpm` is installed: `corepack enable pnpm@latest`
3. Install `npm` packages: `pnpm install`
4. To run the front end locally: `pnpm run start`


### In Docker

1. Make sure Docker is installed and running.
2. While in the root of the KAFE repo, run:
   ```bash
   docker compose -f ./docker-compose.base.yml -f ./docker-compose.local.yml up
   ```
3. If you made changes to Marten projections, you may want to rebuild all of them.
   On Linux, run:
   ```bash
   REBUILD_PROJECTIONS=true docker compose -f ./docker-compose.base.yml -f ./docker-compose.local.yml up
   ```
   On Windows (in PowerShell), run:
   ```powershell
   $env:REBUILD_PROJECTIONS=$true
   docker compose -f ./docker-compose.base.yml -f ./docker-compose.local.yml up
   ```
4. Web is now running at `http://localhost:3000` and the API at `https://localhost:44369`.
5. To stop the containers, run:
   ```bash
   docker compose -f ./docker-compose.base.yml -f ./docker-compose.local.yml down
   ```

### Configuration

Your locally running instance of KAFE is configured using the `appsettings.json` and `appsettings.local.json` files.
The `appsettings.local.json` file is more specific and thus overrides `appsettings.json`.

By default, an admin account registered to `kafe@example.com` is created.
To log in, use this email address in the front-end's UI or in a call to `TemporaryAccountCreationEndpoint` (POST to `/api/v1/tmp-account`).
Then, look into back end's console output and look for the confirmation email.
If you want to log in using the front end, just copy paste the link into the browser.
If you're using the API (e.g., through Postman), change the URL to match the `TemporaryAccountConfirmationEndpoint`.
That is, change the request from `https://<your KAFE instance>/account/token/:token` to `/api/v1/tmp-acount/:token`, where `:token` is the login token.
Now, your user agent of choice should have a valid session cookie.
