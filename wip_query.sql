CREATE OR REPLACE FUNCTION kafe_get_author_perms(
	IN author_id character varying,
	IN account_id character varying
) RETURNS int AS $$
DECLARE
	account_perms jsonb;
	author jsonb;
	perms int := 0;
BEGIN
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
	
	IF account_perms -> '*' IS NOT NULL THEN
		perms := perms | (account_perms -> '*')::int;
	END IF;
	
	IF account_perms -> author_id IS NOT NULL THEN
		perms := perms | (account_perms -> author_id)::int;
	END IF;
	
	RETURN perms;
END;
$$ LANGUAGE plpgsql;

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

CREATE OR REPLACE FUNCTION kafe_get_project_perms_core(
	IN project jsonb,
	IN account jsonb
) RETURNS int AS $$
DECLARE
	perms int := 0;
BEGIN
	IF project IS NULL THEN
		RAISE 'Project not found.';
	END IF;
	
	IF project -> 'GlobalPermissions' IS NOT NULL THEN
		perms := perms | (project -> 'GlobalPermissions')::int;
	END IF;

	IF account -> 'Permissions' -> '*' IS NOT NULL THEN
		perms := perms | (account -> 'Permissions' -> '*')::int;
	END IF;

	IF account -> 'Permissions' -> (project ->> 'Id') IS NOT NULL THEN
		perms := perms | (account -> 'Permissions' -> (project ->> 'Id'))::int;
	END IF;
						 
	IF (account -> 'Permissions' -> (project ->> 'ProjectGroupId'))::int & 4 != 0 THEN
		perms := perms | 2;
	END IF;
	
	RETURN perms;
END
$$ LANGUAGE plpgsql;

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

CREATE OR REPLACE FUNCTION kafe_get_artifact_perms(
	IN artifact_id character varying,
	IN account_id character varying
) RETURNS int AS $$
DECLARE
	project_perms int;
	playlist_perms int;
	account jsonb;
	perms int := 0;
BEGIN
	IF artifact_id IS NULL THEN
		RAISE '`artifact_id` must be non-null.';
	END IF;
	
	account := (SELECT data FROM mt_doc_accountinfo WHERE data ->> 'Id' = account_id);
	
	FOR project_perms IN
		SELECT kafe_get_project_perms_core(data, account)
		FROM mt_doc_projectinfo
		WHERE EXISTS (SELECT 1
				      FROM jsonb_array_elements(data -> 'Artifacts') AS artifact
					  WHERE artifact ->> 'Id' = artifact_id)
	LOOP
		perms := perms | project_perms;
	END LOOP;
	
	RETURN perms;
END;
$$ LANGUAGE plpgsql;

-- SELECT data ->> 'Id' as id FROM mt_doc_projectinfo
-- WHERE kafe_get_project_perms(data ->> 'Id', 'kI1xT3L-tkD') & 1 != 0;

-- SELECT data ->> 'Id' as id, kafe_get_project_perms(data ->> 'Id', 'kI1xT3L-tkD') as perms FROM mt_doc_projectinfo;

-- SELECT data ->> 'Id' as id, kafe_get_project_perms(data ->> 'Id', 'kI1xT3L-tkD') as perms
-- FROM mt_doc_projectinfo
-- WHERE kafe_get_project_perms(data ->> 'Id', 'kI1xT3L-tkD') != 0;

-- SELECT data ->> 'Id' as id, kafe_get_project_perms(data ->> 'Id', '0yfJ-0r_KLp') as perms
-- FROM mt_doc_projectinfo
-- WHERE kafe_get_project_perms(data ->> 'Id', '0yfJ-0r_KLp') != 0;

-- SELECT data ->> 'Id' as id, kafe_get_author_perms(data ->> 'Id', '0yfJ-0r_KLp') as perms
-- FROM mt_doc_authorinfo
-- WHERE kafe_get_author_perms(data ->> 'Id', '0yfJ-0r_KLp') != 0;

-- SELECT data ->> 'Id' as id, kafe_get_artifact_perms(data ->> 'Id', 'kI1xT3L-tkD') as perms
-- FROM mt_doc_artifactinfo
-- WHERE kafe_get_artifact_perms(data ->> 'Id', 'kI1xT3L-tkD') != 0;

SELECT data ->> 'Id' as id, perms.v
FROM mt_doc_artifactinfo, LATERAL (SELECT kafe_get_artifact_perms(data ->> 'Id', 'kI1xT3L-tkD') as v) as perms
WHERE perms.v != 0;

-- SELECT data ->> 'Id' as id, kafe_get_project_perms(data ->> 'Id', NULL) as perms
-- FROM mt_doc_projectinfo
-- WHERE kafe_get_project_perms(data ->> 'Id', NULL) != 0;

-- SELECT * FROM mt_doc_projectinfo, LATERAL (SELECT kafe_get_project_perms(data ->> 'Id', '0K7NZ5rQhrr') as perms)
-- WHERE perms != 0;
