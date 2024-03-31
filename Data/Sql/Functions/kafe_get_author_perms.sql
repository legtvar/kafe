CREATE OR REPLACE FUNCTION {databaseSchema}.kafe_get_author_perms(
	IN author_id character varying,
	IN account_id character varying
) RETURNS int AS $$
DECLARE
	account_perms jsonb;
	author jsonb;
	perms int := 0;
BEGIN
	SET search_path TO {databaseSchema};

	IF author_id IS NULL THEN
		RAISE '`author_id` must be non-null.';
	END IF;
	
	author := (SELECT data
				FROM mt_doc_authorinfo
				WHERE data ->> 'Id' = author_id);
	
	IF author IS NULL THEN
		RAISE 'Author not found.';
	END IF;
	
	IF author -> 'GlobalPermissions' IS NOT NULL THEN
		perms := perms | (author -> 'GlobalPermissions')::int;
	END IF;
	
	IF account_id IS NOT NULL THEN
		account_perms := (SELECT data -> 'Permissions'
				      FROM mt_doc_accountinfo
					  WHERE data ->> 'Id' = account_id);
	END IF;
	
	IF account_perms -> 'system' IS NOT NULL THEN
		perms := perms | (account_perms -> 'system')::int;
	END IF;
	
	IF account_perms -> author_id IS NOT NULL THEN
		perms := perms | (account_perms -> author_id)::int;
	END IF;
	
	RETURN perms;
END;
$$ LANGUAGE plpgsql;
