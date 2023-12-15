CREATE OR REPLACE FUNCTION kafe_get_account_perms(
	IN account_id character varying,
	IN accessing_account_id character varying
) RETURNS int AS $$
DECLARE
	accessing_account_perms jsonb;
	account jsonb;
	perms int := 0;
BEGIN
	IF account_id IS NULL THEN
		RAISE '`account_id` must be non-null.';
	END IF;
	
	account := (SELECT data
				FROM mt_doc_accountinfo
				WHERE data ->> 'Id' = account_id);
	
	IF account IS NULL THEN
		RAISE 'Account not found.';
	END IF;

	IF accessing_account_id IS NOT NULL THEN
		accessing_account_perms := (SELECT data -> 'Permissions'
									FROM mt_doc_accountinfo
									WHERE data ->> 'Id' = accessing_account_id);
	END IF;
	
	IF accessing_account_perms -> 'system' IS NOT NULL THEN
		perms := perms | (accessing_account_perms -> 'system')::int;
	END IF;
	
	IF accessing_account_perms -> account_id IS NOT NULL THEN
		perms := perms | (accessing_account_perms -> account_id)::int;
	END IF;

	RETURN perms;
END;
$$ LANGUAGE plpgsql;
