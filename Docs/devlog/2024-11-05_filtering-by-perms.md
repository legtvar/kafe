# Filtering by Permissions

Marten (at least as I am writing this) doesn't support the `Join()` LINQ operator.
Which sucks.
Because I'd rather like to do query like:

```sql
SELECT
    ROW(entity.*),
    ROW(perms.*)
FROM
    public.mt_doc_playlistinfo AS entity
INNER JOIN
    public.mt_doc_entitypermissioninfo AS perms
ON
    entity.id = perms.id
WHERE
    ((perms.data -> 'AccountEntries' -> 'nDa053kyH4E' -> 'EffectivePermission')::int & 1) = 1
```

Good news is that Marten still allows me to run a query like this through its
[`AdvancedSql` methods](https://martendb.io/documents/querying/advanced-sql.html).
Bad news is that these methods no longer produce an `IQueryable`, meaning that they cannot be augmented further by,
for example, sorting or paging.
And they cannot be (efficiently) used in
[Hot Chocolate](https://chillicream.com/docs/hotchocolate/v14/integrations/marten),
should I ever want to support GraphQl.

Right now, it doesn't really matter, I can just the query verbatim as a string
(in `KafeQueryable.GetEntitiesWithPermission`).
However, this will get in the way eventually.
I see two solutions:

1. Teach Marten about `Join()`. (very difficult)
2. Implement our own little `IQueryable`. (seems a little insane but who knows it might be the better option)

## A few moments later...

> Don't you love it when you have an idea just as you wrote about how hopeless the situation is?

Yesterday, I though that using the good old [`MatchesSql` method](https://martendb.io/documents/querying/linq/sql.html)
results in a horrible three-second-long query.
I was wrong.
It's just a little worse that the `JOIN`-featuring query above.
I compared them (not very scientifically) in PgAdmin by running the following queries several times:

Option `JOIN`:

```sql
SELECT projects.data, perms.data FROM mt_doc_projectinfo as projects
JOIN mt_doc_entitypermissioninfo as perms ON projects.id = perms.id
WHERE (perms.data -> 'AccountEntries' -> 'Ugc5qcZtLLp' -> 'EffectivePermission')::int != 0 
```

Option `MatchesSql`:

```sql
SELECT * FROM mt_doc_projectinfo as p
WHERE (
    (SELECT data FROM mt_doc_entitypermissioninfo AS perms WHERE perms.id = p.id)
    -> 'AccountEntries' -> 'Ugc5qcZtLLp' -> 'EffectivePermission')::int != 0
```

`JOIN` took between 38 and 65 ms (~40 ms avg). `MatchesSql` took between 47 and 99 ms (~50 ms avg).
The 10 ms performance hit is a sacrifice I'm willing to make in order to not have to implement
anything related to `IQueryable`.


## Even later...

I forgot about `GlobalPermission`. :D

```sql
SELECT * FROM mt_doc_projectinfo as p
WHERE
    (
        SELECT
            (data -> 'AccountEntries' -> 'Ugc5qcZtLLp' -> 'EffectivePermission')::int
            | (data -> 'GlobalPermission')::int
        FROM mt_doc_entitypermissioninfo AS perms
        WHERE perms.id = p.id
    ) != 0
```
