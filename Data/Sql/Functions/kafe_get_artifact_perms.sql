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
