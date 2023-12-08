CREATE OR REPLACE FUNCTION kafe_get_projectgroup_perms(
	IN projectgroup_id character varying,
	IN account_id character varying
) RETURNS int AS $$
DECLARE
	account_perms jsonb;
	projectgroup jsonb;
	perms int := 0;
BEGIN
	IF projectgroup_id IS NULL THEN
		RAISE '`projectgroup_id` must be non-null.';
	END IF;
	
	projectgroup := (SELECT data
				FROM mt_doc_projectgroupinfo
			    WHERE data ->> 'Id' = projectgroup_id);
				
	IF projectgroup IS NULL THEN
		RAISE 'Project group not found.';
	END IF;
	
	IF projectgroup -> 'GlobalPermissions' IS NOT NULL THEN
		perms := perms | (projectgroup -> 'GlobalPermissions')::int;
	END IF;
	
	IF account_id IS NOT NULL THEN
		account_perms := (SELECT data -> 'Permissions'
				      FROM mt_doc_accountinfo
					  WHERE data ->> 'Id' = account_id);
	END IF;

	IF account_perms -> '*' IS NOT NULL THEN
		perms := perms | (account_perms -> '*')::int;
	END IF;

	IF account_perms -> projectgroup_id IS NOT NULL THEN
		perms := perms | (account_perms -> projectgroup_id)::int;
	END IF;
	
	RETURN perms;

END;
$$ LANGUAGE plpgsql;
