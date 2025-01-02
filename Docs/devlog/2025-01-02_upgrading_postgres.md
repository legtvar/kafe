# Upgrading Postgres

While writing the `ProjectCreatorInspectPermission` event correction, I decided to use the [`JSON_EXISTS`] function.
Little did I know that this function was only added in PostgreSQL 17.
Thus, I got dragged into upgrading Postgres on staging from version 16 to 17.

And since I know that this is likely not the last time, I have to ugprade Postgres to a new major version,
this is how I did it today:

1. Bumped the Docker image in `~/docker-compose-base.yml` from `16-alpine` to `17-alpine`.
2. Ran the `deploy-staging` job.
3. Realized API cannot connect to the DB because Postgres errors out with `The data directory was initialized by PostgreSQL version 16, which is not compatible with this version 17.2.`
4. Stopped the `kafe-staging-db-1` container.
5. Created the `kafe_staging_postgres_17` volume.
6. Ran [`tianon/docker-postgres-upgrade`]:
```bash
docker run --rm -it \
    -v kafe_staging_postgres:/var/lib/postgresql/16/data \
    -v kafe_staging_postgres_17:/var/lib/postgresql/17/data \
    "tianon/postgres-upgrade:16-to-17"
```
7. Removed `/var/lib/docker/volumes/kafe_staging_postgres`.
8. Renamed `/var/lib/docker/volumes/kafe_staging_postgres_17` to `kafe_staging_postgres`.
9. `docker volume rm kafe_staging_postgres_17`
10. Appended `host all all all scram-sha-256` at the end of `pg_hba.conf` because it got removed in the upgrade.
11. Restarted `docker-staging-db-1` and `docker-staging-api-1`.

[`JSON_EXISTS`]: https://www.postgresql.org/docs/17/functions-json.html#SQLJSON-QUERY-FUNCTIONS
[`tianon/docker-postgres-upgrade`]: https://github.com/tianon/docker-postgres-upgrade
