


CREATE OR REPLACE FUNCTION kafe_get_project_perms(
	IN project_id character varying,
	IN account_id character varying
) RETURNS int AS $$
DECLARE
	account jsonb;
	project jsonb;
BEGIN
	IF project_id IS NULL THEN
		RAISE '`project_id` must be non-null.';
	END IF;
	
	project := (SELECT data
				FROM mt_doc_projectinfo
			    WHERE data ->> 'Id' = project_id);

	account := (SELECT data
				FROM mt_doc_accountinfo
				WHERE data ->> 'Id' = account_id);

	
	RETURN kafe_get_project_perms_core(project, account);
END;
$$ LANGUAGE plpgsql;
