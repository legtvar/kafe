# KAFE browser E2E tests

These xUnit tests use Microsoft.Playwright to exercise the real KAFE frontend, API,
PostgreSQL database, FFmpeg analysis, VP9/Opus conversion, and HTML5 video playback.

Coverage:

- Temporary account creation: the confirmation email creates a temporary account and
  establishes a session for it.
- Temporary account login: a second confirmation logs into the same existing temporary
  account instead of creating another account.
- Admin group creation: the seeded administrator creates a project group through the
  frontend and reaches its detail page.
- MP4 and MKV: temporary-account login, open project group, project creation,
  artifact creation, video upload, conversion, artifact preview, and actual playback
  (`readyState >= 2`, unpaused, and `currentTime > 0`).
- MOV: the unsupported `video/quicktime` upload enters the visible failure state and
  can be reset for another attempt.
- Corrupt MP4: media cannot reach a playable state and the browser shows the player
  error instead of playing.
- Anonymous visitor: the authenticated project-group route redirects to login.

Browser tests are in their own `Kafe.Tests.EndToEnd` project, separate from the
unit/API tests in `Tests`. They provision their own isolated application stack.

## Run with Testcontainers

Chromium and its system dependencies are installed at startup through the
[Playwright .NET API](https://playwright.dev/dotnet/docs/browsers#install-browsers-via-api)
(`Microsoft.Playwright.Program.Main`). No separate browser-install command is needed.

From the repository root:

```sh
dotnet test Tests.EndToEnd/Kafe.Tests.EndToEnd.csproj
```

Testcontainers starts the repository's existing `docker-compose.base.yml` and
`docker-compose.local.yml`. The API, Vite builder, and real Blender images are built
sequentially using the repository's Dockerfiles. Their images are retained for layer
caching across runs. PostgreSQL, API, Vite, and the real Blender service are then
started with Compose's `--no-build` option.

One small generated Compose override selects those images and configures the HTTPS
frontend, PostgreSQL health check, and one-second conversion polling interval. A
generated Vite proxy configuration forwards browser API requests to the real API
container. No additional Dockerfiles, nginx configuration, application settings files,
or Compose files are checked in. Failure screenshots/traces and JUnit output go to
`Tests.EndToEnd/artifacts/`.

The local Compose files use fixed `kafe_local_*` volume names. As with a normal
`docker compose down --volumes`, Testcontainers removes those volumes on teardown to
leave the next run with a fresh database. Run this suite only against a Docker daemon
whose local KAFE volumes are disposable (which is the normal setup on CI).

## GitLab CI

The `test-e2e` job in the repository's `.gitlab-ci.yml` runs this project.

The job uses `docker:dind-rootless` without TLS and sets `KAFE_E2E_HOST=docker` and
`TESTCONTAINERS_HOST_OVERRIDE=docker`, because ports published by the DinD daemon
belong to the `docker` service rather than to the test runner's `localhost`. Ryuk is
disabled for rootless compatibility; the fixture explicitly removes its stack while
preserving cached images. Test startup installs Chromium through the Playwright .NET API.

The repository's path must be identical in the test container and DinD service (as it
is with GitLab's mounted project directory), so Docker Compose can resolve its build
context. Testcontainers provides PostgreSQL and all application services, so no
persistent CI services or manual seed database are required.

The only database access in test setup is through the real application. Confirmation
emails are read from the disposable API container's logs. No API endpoints, project
data, upload responses, conversion results, or player behavior are mocked.
