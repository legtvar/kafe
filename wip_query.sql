CREATE OR REPLACE FUNCTION kafe_get_project_perms(
	IN project_id character varying,
	IN account_id character varying
) RETURNS int AS $$
DECLARE
	account_perms jsonb;
	project_group_id character varying;
	perms int := 0;
BEGIN
	IF project_id IS NULL THEN
		RAISE 'Project id must not be null!';
	END IF;

	IF account_id IS NOT NULL THEN
		account_perms := (SELECT data -> 'Permissions'
				      FROM mt_doc_accountinfo
					  WHERE data ->> 'Id' = account_id);
	END IF;
	
	IF account_perms IS NULL THEN
		RETURN 0;
	END IF;

	IF account_perms -> project_id IS NOT NULL THEN
		RETURN (account_perms -> project_id)::int;
	END IF;
	
	project_group_id := (SELECT data ->> 'ProjectGroupId'
						 FROM mt_doc_projectinfo
						 WHERE data ->> 'Id' = project_id);
						 
	IF (account_perms -> project_group_id)::int & 4 != 0 THEN;
		perms := perms | 2;
	
	
	RETURN 0;
END;
$$ LANGUAGE plpgsql;

-- SELECT data ->> 'Id' as id FROM mt_doc_projectinfo
-- WHERE kafe_get_project_perms(data ->> 'Id', 'kI1xT3L-tkD') & 1 != 0;

-- SELECT data ->> 'Id' as id, kafe_get_project_perms(data ->> 'Id', 'kI1xT3L-tkD') as perms FROM mt_doc_projectinfo;

SELECT data ->> 'Id' as id, kafe_get_project_perms(data ->> 'Id', 'awsIwAfNBTv') as perms FROM mt_doc_projectinfo
WHERE kafe_get_project_perms(data ->> 'Id', 'awsIwAfNBTv') != 0;

-- SELECT * FROM mt_doc_projectinfo, LATERAL (SELECT kafe_get_project_perms(data ->> 'Id', '0K7NZ5rQhrr') as perms)
-- WHERE perms != 0;
