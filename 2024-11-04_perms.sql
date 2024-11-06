SELECT projects.data, perms.data FROM mt_doc_projectinfo as projects
JOIN mt_doc_entitypermissioninfo as perms ON projects.id = perms.id
WHERE (perms.data -> 'AccountEntries' -> 'Ugc5qcZtLLp' -> 'EffectivePermission')::int != 0 

-- SELECT data from mt_doc_accountinfo
-- where data -> 'Permissions' ->> 'system' is not null

-- SELECT data FROM mt_doc_entitypermissioninfo
-- 	CROSS JOIN LATERAL jsonb_object_keys(data -> 'AccountEntries') as entries
-- WHERE jsonb_array_length(data -> 'ParentIds') != 0
-- 	AND data -> 'AccountEntries' ->> 'tNRkraybfb1' is not null
-- GROUP BY id
-- HAVING count(*) > 3
