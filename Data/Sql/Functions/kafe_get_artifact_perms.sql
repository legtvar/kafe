CREATE OR REPLACE FUNCTION {databaseSchema}.kafe_get_artifact_perms(
	IN artifact_id character varying,
	IN account_id character varying
) RETURNS int AS $$
DECLARE
	project_perms int;
	playlist_perms int;
	account jsonb;
	perms int := 0;
BEGIN
	SET search_path TO {databaseSchema};

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
	
	IF perms & 1 = 0 THEN
		FOR playlist_perms IN
			SELECT kafe_get_playlist_perms_core(data, account)
			FROM mt_doc_playlistinfo
			WHERE EXISTS (SELECT 1
						FROM jsonb_array_elements(data -> 'EntryIds') AS artifact
						WHERE artifact ->> 'Id' = artifact_id)
		LOOP
			IF playlist_perms & 1 = 1 THEN
				perms := perms | 1;
				EXIT;
			END IF;
		END LOOP;
	END IF;
	
	RETURN perms;
END;
$$ LANGUAGE plpgsql;
