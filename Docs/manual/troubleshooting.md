---
title: Troubleshooting
tags: doc
priority: 015
---

# Troubleshooting

**The API cannot connect to the DB.**

Make sure Postgres is installed and running.
On Windows, use the Task Manager and in the Services pane look for a services called `postgres-x64-17`.
It's status must be running.
On Linux, use `systemctl`.

Make sure Postgres is using port **5432**.
If you've upgraded Postgres from an earlier version, the port could have changed.
Check that your `postgresql.conf` config, which on Windows is at `C:\Program Files\PostgreSQL\17\data`, contains this line:

```conf
port = 5432 # (change requires restart)
```

> Change indeed requires restart. On Windows, right-clich on the Service in Task Manager and press Restart.
