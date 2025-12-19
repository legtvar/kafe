---
title: Getting Started
tags: doc
priority: 010
---

# Getting Started

You have a few options of how to set up your developer environment. If you run into issues, check the instructions below and [Troubleshooting](./troubleshooting.md).

## Dev Container

Click on the image below to clone the repository and open it in a dev container. For it to work you must have Docker installed and set up for Linux containers.

[![KAFE Dev Container](https://img.shields.io/static/v1?label=KAFE%20Dev%20Container&message=Open&color=blue)](https://vscode.dev/redirect?url=vscode://ms-vscode-remote.remote-containers/cloneInVolume?url=https://gitlab.fi.muni.cz/legtvar/kafe)

Alternatively:

1. Install Docker and set it up for Linux containers.
2. Clone the repository.
3. Open the repository in Visual Studio Code and run the _"Dev Containers: Reopen in Container"_ command.

When you have your dev container set up, you're ready to run KAFE:
1. To run the web UI and have it connect to the staging environment, go to `/kafe/Web` and run `pnpm run dev`.
2. To run the API locally, go to `/kafe/Api` and run `dotnet run`.
3. To run the web UI and have it connect to the locally running API, go to `/kafe/Web` and run `pnpm run start`.
4. The web UI will be accessible at https://localhost:3000 and the API at https://localhost:44369.

## Docker Compose

If you need an environment closer to what runs on staging or production, you can also run KAFE using [`docker compose`](https://docs.docker.com/compose/):

1. Install Docker and set it up for Linux containers.
2. Clone the repository.
3. Run:
   ```bash
   docker compose -f docker-compose.base.yml -f docker-compose.local.yml build
   docker compose -f docker-compose.base.yml -f docker-compose.local.yml up
   ```
4. The web UI will be accessible at https://localhost:3000 and the API at https://localhost:44369.

## Local Development

You can also run KAFE without Docker. This has the advantage of being easier to debug. However, it takes longer to set up and may conflict with your local configs if you have other projects using .NET, Node.js, or Postgres.

1. Install the dependencies below.
2. Clone the repository.
3. Make sure PostgreSQL is running at port 5432
   If it's running on a different port, update connection strings in `Api/appsettings.local.json`.
4. Make sure you have `pnpm` is installed: `npm install -g pnpm`.
4. Install web UI dependencies by running `pnpm install` in the `Web` and `Web/proxy` directories.
5. Install API dependencies by running `dotnet restore` in the `Api` directory.
6. To run the web UI and have it connect to the staging environment, go to `Web` and run `pnpm run dev`.
7. To run the API locally, go to `Api` and run `dotnet run`.
8. To run the web UI and have it connect to the locally running API, go to `Web` and run `pnpm run start`.
9. The web UI will be accessible at https://localhost:3000 and the API at https://localhost:44369.


## Dependencies

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

## Logging in

To log in:

1. Use the `kafe@example.com` email address in the front-end's UI or in a call to `TemporaryAccountCreationEndpoint` (POST to `/api/v1/tmp-account`). This is the default admin account.
2. Look into back end's console output and look for the confirmation email.
3. Use the confirmation token from the confirmation email.
   - If you want to log in using the front end, just copy paste the link into the browser.
   - If you're using the API (e.g., through Postman), change the URL to match the `TemporaryAccountConfirmationEndpoint`. That is, change it:
   ```
   ORIGINAL:
   https://<your KAFE instance>/account/token/:token
   NEW:
   https://<your KAFE instance>/api/v1/tmp-account/:token
   ```
   where `:token` is the confirmation token.

4. Now, your browser should have a valid session cookie.


## Rebuilding projections

If you've made changes to Marten projectsion, you may need to rebuild them.

### Locally or in Dev Container

In `Api`, run `dotnet run -- projections rebuild --shard-timeout 30m`.

### Docker Compose

On Linux, run:
```bash
docker compose -f ./docker-compose.base.yml -f ./docker-compose.local.yml down
REBUILD_PROJECTIONS=true docker compose -f ./docker-compose.base.yml -f ./docker-compose.local.yml up
```

On Windows run:
```powershell
docker compose -f docker-compose.base.yml -f docker-compose.local.yml down
$env:REBUILD_PROJECTIONS=$true
docker compose -f ./docker-compose.base.yml -f ./docker-compose.local.yml up
```
