# Film upload browser E2E tests

These xUnit tests use Microsoft.Playwright to exercise the real KAFE frontend, API,
PostgreSQL database, FFmpeg analysis, VP9/Opus conversion, and HTML5 video playback.

Coverage:

- MP4 and MKV: temporary-account login, open project group, project creation,
  artifact creation, video upload, conversion, artifact preview, and actual playback
  (`readyState >= 2`, unpaused, and `currentTime > 0`).
- MOV: the unsupported `video/quicktime` upload enters the visible failure state and
  can be reset for another attempt.
- Corrupt MP4: media cannot reach a playable state and the browser shows the player
  error instead of playing.
- Anonymous visitor: the authenticated project-group route redirects to login.

Browser tests are skipped during an ordinary `dotnet test Tests` unless `KAFE_E2E=1`.
This prevents accidental recreation of the local Docker volumes during unit/API test
runs.

## Run with Testcontainers

Install Chromium before running the suite (adjust the Node runtime path on other platforms):

```sh
Tests/bin/Debug/net9.0/.playwright/node/linux-x64/node \
  Tests/bin/Debug/net9.0/.playwright/package/cli.js install --with-deps --no-shell chromium
```

Then, from the repository root:

```sh
KAFE_E2E=1 dotnet test Tests/Kafe.Tests.csproj --filter FullyQualifiedName~Kafe.Tests.EndToEnd
```

Testcontainers starts the repository's existing `docker-compose.base.yml` and
`docker-compose.local.yml`. It creates the required external `backbone` network and
generates one small Compose override artifact for the HTTPS Vite frontend, PostgreSQL
health check, one-second conversion polling interval, and a lightweight placeholder
for the unused Blender service. No additional Dockerfiles, nginx configuration,
application settings files, or Compose files are checked in.
Failure screenshots/traces and JUnit output go to `Tests/EndToEnd/artifacts/`.

The local Compose files use fixed `kafe_local_*` volume names. As with a normal
`docker compose down --volumes`, Testcontainers removes those volumes on teardown to
leave the next run with a fresh database. Run this suite only against a Docker daemon
whose local KAFE volumes are disposable (which is the normal setup on CI).

## GitLab CI

`Tests/EndToEnd/.gitlab-ci.yml` contains a ready-to-use job. Include it from the root
`.gitlab-ci.yml` when repository policy permits changing that file:

```yaml
include:
  - local: Tests/EndToEnd/.gitlab-ci.yml
```

The job installs Chromium, sets `KAFE_E2E=1`, and invokes the same `dotnet test`
command. Testcontainers provides PostgreSQL and all application services, so no
persistent CI services or manual seed database are required.

The only database access in test setup is through the real application. Confirmation
emails are read from the disposable API container's logs. No API endpoints, project
data, upload responses, conversion results, or player behavior are mocked.
