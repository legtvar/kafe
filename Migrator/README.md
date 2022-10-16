# Migrator

This is a tool meant for migrating data from the old LEMMA systems (such as WMA) into KAFE.

## Entity Framework Model

The Entity Framework mdoel has been scaffolded automatically (with later manual modifications) with this command:

```
dotnet ef dbcontext scaffold "host=localhost;database=lemma;password=password;username=postgres" Npgsql.EntityFrameworkCore.PostgreSQL --data-annotations --output-dir Lemma --namespace Kafe.Lemma --force
```

> You may need to adjust the connection string.
