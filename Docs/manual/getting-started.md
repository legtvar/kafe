# Getting Started

1. Install the prerequisites below.
1. Clone the repository.
2. Optionally [set up a VM](./vm-setup) or [Docker](https://www.docker.com/).
3. Install KAFE using Docker compose or run it locally through `pnpm` or `dotnet` CLI.


## Prerequisites

**Back end (API)**

* [.NET SDK 8.0 or later](https://dotnet.microsoft.com/en-us/download).
* [PostgreSQL](https://www.postgresql.org/)

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


## Running using Docker Compose

Right now, the Docker compose files are configured to either result in the production or staging environment.
You can get it to work locally, if you really want to, but then it's just easier to run it outside of Docker.


## Running locally

**Back end (API)**

1. Open in an editor of your choice.
2. Restore NuGet packages: `dotnet restore`
3. While in the `Api/` dir, to run the API: `dotnet run`
4. Run the front end or use Postman.

**Front end (Web)**

1. Go to the `Web` directory.
2. Ensure `pnpm` is installed: `corepack enable pnpm@latest`
3. Install `npm` packages: `pnpm install`
4. To run the front end locally: `pnpm run start`
