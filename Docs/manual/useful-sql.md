---
title: Useful SQL
tags: doc
priority: 80
---

# Useful SQL queries

## Find projects referencing a certain artifact

```sql
SELECT * FROM mt_doc_projectinfo
WHERE 'UO20OFQkWAx' IN (SELECT sub ->> 'Id'
                        FROM jsonb_array_elements(data -> 'Artifacts') AS sub)
```

## Find artifacts in a project group 'CafMk2sO9fL'

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

## Drop `test*` schemas in Postgres

```sql
DO $$
DECLARE
    s text;
BEGIN
    FOR s IN 
        SELECT schema_name 
        FROM information_schema.schemata 
        WHERE schema_name LIKE 'test%'
    LOOP
        EXECUTE format('DROP SCHEMA IF EXISTS %I CASCADE', s);
        RAISE NOTICE 'Dropped schema: %', s;
    END LOOP;
END $$ LANGUAGE plpgsql;
```
