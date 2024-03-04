```
      █      █                                                                              
     █      █                                                                               
      █      █         █  █▀ ██   ▄████  ▄███▄       ██▄   ▄███▄      ▄   █    ████▄   ▄▀   
                       █▄█   █ █  █▀   ▀ █▀   ▀      █  █  █▀   ▀      █  █    █   █ ▄▀     
  ███████████████      █▀▄   █▄▄█ █▀▀    ██▄▄        █   █ ██▄▄   █     █ █    █   █ █ ▀▄   
  █            ███     █  █  █  █ █      █▄   ▄▀     █  █  █▄   ▄▀ █    █ ███▄ ▀████ █   █  
   █           ███       █      █  █     ▀███▀       ███▀  ▀███▀    █  █      ▀       ███   
    █         █  █      ▀      █    ▀                                █▐                     
     █████████                ▀                                      ▐                      
```

> A very minimalistic [_architecture design record_](https://github.com/joelparkerhenderson/architecture-decision-record).

# Architecture Decisions

## `mlejnek` (2024-03-04)

Since I need to describe the process of creating KAFE's new VM to the admins of LEMMA RS, I might as well put it here:

KAFE's new home is called `mlejnek` and it is a VM in [Stratus.FI](https://www.fi.muni.cz/tech/unix/stratus.html).
It has these specs:

- Debian 12
- 8 GB RAM
- 2 GB of swap space
- 64 GB system storage
- 2 TB archive storage

We use the following software for KAFE's basic operation:

- _GitLab Runner_ for running CI jobs.
- _Docker_ for KAFE's individual containers.
- _Portainer_ for management of Docker containers.
- _Traefik_ as a reverse proxy configured by container labels. It also obtains certs from Let's Encrypt.
- _FFMpeg_ as a provider of video metadata and for converting videos.

Once we had this basic setup, we communicated with CVT FI to set up the following services:

- Put the VM into production mode:
    - Get a public IPv4 and IPv6.
    - Change the owner of the VM to an employee of FI.
    - Set up _Nagios_ for automatic notification of downtime.
    - Point `kafe-stage.fi.muni.cz` at the VM.
- New group in faculty administration, `adm_kafe`, that manages `mail_kafe`.
- New group in GitLab - `LeGTVaR` - that is synced to `mail_kafe`.
- _Bacula_ to backup of the archival storage and the VM itself through.
- CNAME records for `kafe-stage.fi.muni.cz` and `kafe.lemma.fi.muni.cz`. Both point to `kafe.fi.muni.cz`

Finally for the Games extension, we contacted ICS MU, who then pointed `games.muni.cz` to a nameserver of FI.
CVT then added a CNAME from `games.muni.cz` to `kafe.fi.muni.cz`.


## Use `ProblemDetails` (2024-03-03)

With KAFE finally safely resting in its new home, `mlejnek` on Stratus.FI, I finally have time to work directly on KAFE.

I bumped into a bug with the `TemporaryAccountClosed` event.
Marten had no knowledge of it, yet it was in the database.
This happened because, I removed the event type from the C# codebase back in December.
The fix was simple enough: I added an upcaster from `TemporaryAccountClosed` to `TemporaryAccountRefreshed`.

However, there was also an issue with the app not logging the error, so I had to take a look at
[ASP.NET Core's error handling](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling).
Based on that, I realized that the JSON that ASP.NET Core returns is actually standardized as
[RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807).
So now we use `Microsoft.AspNetCore.Mvc.ProblemDetails` and log the error as well.

## Static `Create`s (2023-12-16)

Made the `Create` methods on all projections `static`.

Turns out this is necessary when using Marten. (Thanks, Oskar.)

## System Hrib (2023-12-15)

Changed the system `Hrib` from `'*'` to `"system"` because `'*'` is weird in URLs.

# History

These decisions were made before we started writing this devlog.
Nevertheless, they are important to remember.

## Hrib

We use string Human-Readable Identifier Ballast (`Hrib`) for Ids on pretty much everything.
These are essentially YouTube's 11-chars-long Ids but without the checks for swear words.


# Other

## `pg_dump`

To dump all:

```bash
sudo -u postgres pg_dumpall > lemma-yyyy-MM-dd-all.sql
```

To dump and tar WMA:

```bash
NAME=lemma-yyyy-MM-dd pg_dump --format d --file "/tmp/$NAME" lemma && tar -czf "/tmp/$NAME.tar.gz" -C "/tmp/$NAME" .
```

To restore WMA:

```bash
NAME=lemma-yyyy-MM-dd
mkdir $NAME
tar -xzvf $NAME.tar.gz -C $NAME
pg_restore -U postgres --clean --create --dbname postgres --format d lemma-2023-12-16
```

> NOTE: `--dbname postgres` is there because the first thing the restore does is `CREATE DATABASE lemma`. See [this](https://stackoverflow.com/questions/40784677/pg-restore-with-c-option-does-not-create-the-database).


## Useful SQL queries

To find projects referencing a certain artifact:

```sql
SELECT * FROM mt_doc_projectinfo
WHERE 'UO20OFQkWAx' IN (SELECT sub ->> 'Id'
                        FROM jsonb_array_elements(data -> 'Artifacts') AS sub)
```

To find artifacts in a project group 'CafMk2sO9fL':

```sql
select id from mt_doc_videoshardinfo
where data ->> 'ArtifactId' in
	(select data ->> 'Id' from mt_doc_artifactdetail
	where data -> 'ContainingProjectIds' ->> 0 in 
	(select data ->> 'Id' from mt_doc_projectinfo
	 where data ->> 'ProjectGroupId' = 'CafMk2sO9fL'
))
```

...and to get their collective file size:

```sql
select sum((data -> 'Variants' -> 'sd' ->> 'FileLength')::int) from mt_doc_videoshardinfo
where data ->> 'ArtifactId' in
	(select data ->> 'Id' from mt_doc_artifactdetail
	where data -> 'ContainingProjectIds' ->> 0 in 
	(select data ->> 'Id' from mt_doc_projectinfo
	 where data ->> 'ProjectGroupId' = 'CafMk2sO9fL'
))
```

## VM Setup

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

## Useful Commands

### Get an IP address of a container

To get the IP address of the Postgres container to use in PgAdmin remotely:

```bash
docker inspect -f '{{.NetworkSettings.Networks.backbone.IPAddress}}' kafe-db-1
```

### Create an overlay

To create an overlay filesystem, where data in `lowerdir` is read-only, data in `upperdir` contains the changes, `workdir` is for files while they're being changed:

```bash
 mount -t overlay overlay -o lowerdir=/data/kafe/temp,upperdir=/data/kafe-stage/upper-temp,workdir=/data/kafe-stage/work-temp /data/kafe-stage/temp
```

...and the final mountpoint is `/data/kafe-stage/temp`.
