# Useful Commands

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

> NOTE: `--dbname postgres` is there because the first thing the restore does is `CREATE DATABASE lemma`.
>       See [this](https://stackoverflow.com/questions/40784677/pg-restore-with-c-option-does-not-create-the-database).

## Get an IP address of a container

To get the IP address of the Postgres container to use in PgAdmin remotely:

```bash
docker inspect -f '{{.NetworkSettings.Networks.backbone.IPAddress}}' kafe-db-1
```

## Create an overlay

To create an overlay filesystem, where data in `lowerdir` is read-only, data in `upperdir` contains the changes, `workdir` is for files while they're being changed:

```bash
 mount -t overlay overlay -o lowerdir=/data/kafe/temp,upperdir=/data/kafe-stage/upper-temp,workdir=/data/kafe-stage/work-temp /data/kafe-stage/temp
```

...and the final mountpoint is `/data/kafe-stage/temp`.

## Remote Debugging

To remote debug staging, use this VSCode's `launch.json` config:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Attach to Staging",
      "type": "coreclr",
      "request": "attach",
      "processId": "${command:pickRemoteProcess}",
      "pipeTransport": {
        "pipeCwd": "${workspaceFolder}",
        "pipeProgram": "ssh",
        "pipeArgs": [
          "-T",
          "kafe.fi.muni.cz",
          "docker",
          "exec",
          "-i",
          "kafe-staging-api-1",
          "sh",
          "-c"
        ],
        "debuggerPath": "~/vsdbg/vsdbg"
      },
      "sourceFileMap": {
        "/kafe/src": "${workspaceRoot}"
      },
      "justMyCode": false
    }
  ]
}
```
