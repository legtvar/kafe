---
title: VM Setup
tags: doc
priority: 100
---

# VM Setup

You should be able to set up KAFE on any Linux machine with Docker installed using the `docker-compose.yml` file.
It does need there to be some Docker volumes. These volumes are marked as `external` in `docker-compose.yml`:

- `kafe_archive` - Storage for all the primary "original" artifacts.
- `kafe_generated` - Storage for all the generated artifacts.
- `kafe_temp` - Storage for temporary stuff.
- `kafe_postgres` - Storage for the DB data.
- `kafe_secrets` - Storage currently only for ASP.NET Core's signing keys.

To create these volumes use the following command (assuming you have a large and backed-up storage available at `/data/`):

```bash
$name = "sample"
docker volume create --driver local --opt type=none --opt device=/data/kafe/$name --opt o=bind kafe_$name
```

> NOTE: This only works on Linux. The `--opt` options are taken from [`mount(8)`](https://man7.org/linux/man-pages/man8/mount.8.html). See [Docker docs](https://docs.docker.com/engine/reference/commandline/volume_create/#opt) for more details.

> NOTE: This solution is based on [this SO question](https://stackoverflow.com/questions/39496564/docker-volume-custom-mount-point).

To create volumes for staging using Linux's overlay filesystem:

```bash
docker volume create --driver local --opt type=overlay --opt device=overlay --opt o=lowerdir=/data/kafe/temp,upperdir=/data/kafe-stage/upper-temp,workdir=/data/kafe-stage/work-temp kafe_staging_temp
```
