# Mock OIDC identity provider

For local OIDC login testing, KAFE uses [navikt/mock-oauth2-server](https://github.com/navikt/mock-oauth2-server).

Start the server from the repository root:

```bash
docker compose -f MockIdentity/docker-compose.yml up
```

The API's default local configuration discovers it at `http://localhost:44370/default`.
Use the frontend's external login action to open the mock login page.

## Users

The login page accepts any username and optional additional claims as JSON.

| Username | Email | Name |
| --- | --- | --- |
| `alice` | `AliceSmith@email.com` | Alice Smith |
| `bob` | `BobSmith@email.com` | Bob Smith |
| `kafe` | `kafe@example.com` | kafe |
| any other username | `<username>@example.com` | the username |

The `kafe` identity corresponds to the administrator seeded by `Api/appsettings.local.json`.
Claims for these users are configured in `config.json`.

## Docker and dev containers

When both KAFE and the browser must access the mock across a Docker boundary, use
`http://host.docker.internal:44370/default` as the API's `Oidc:Authority` instead.
The browser and API must use the same authority so that the discovery document and
issuer claim match.

On Linux, add `host.docker.internal:host-gateway` to the API container's `extra_hosts`
and add `127.0.0.1 host.docker.internal` to the host's `/etc/hosts`. Docker Desktop
provides the hostname automatically. Forward port `44370` from a dev container when
its browser runs on the host.

This server is only for local testing. It does not strictly validate a client registry,
client secrets, or redirect URIs, and must not be deployed to staging or production.
