CREATE OR REPLACE FUNCTION {databaseSchema}.kafe_get_resource_perms(
	IN resource_id character varying,
	IN account_id character varying
) RETURNS TABLE(is_found boolean, perms int) AS $$
DECLARE
	resource_type character varying;
BEGIN
	SET search_path TO {databaseSchema};

	IF resource_id IS NULL THEN
		RAISE '`resource_id` must be non-null.';
	END IF;
	
	IF resource_id = 'system' THEN
		is_found := TRUE;
		perms := kafe_get_system_perms(account_id);
		RETURN NEXT;
		RETURN;
	END IF;
	
	resource_type := (SELECT type
					  FROM mt_streams
					  WHERE id = resource_id);
	IF resource_type IS NULL THEN
		-- Return false so that no exception is thrown and instead 
		is_found := FALSE;
		perms := 0;
		RETURN NEXT;
		RETURN;
	END IF;
	
	CASE
		WHEN resource_type = 'account_info' THEN
			perms := kafe_get_account_perms(resource_id, account_id);
		WHEN resource_type = 'artifact_info' THEN
			perms := kafe_get_artifact_perms(resource_id, account_id);
		WHEN resource_type = 'author_info' THEN
			perms := kafe_get_author_perms(resource_id, account_id);
		WHEN resource_type = 'project_info' THEN
			perms := kafe_get_project_perms(resource_id, account_id);
		WHEN resource_type = 'project_group_info' THEN
			perms := kafe_get_projectgroup_perms(resource_id, account_id);
		WHEN resource_type = 'playlist_info' THEN
			perms := kafe_get_playlist_perms(resource_id, account_id);
		ELSE
			is_found := FALSE;
			perms := 0;
			RETURN NEXT;
			RETURN;
	END CASE;

	is_found := TRUE;
	RETURN NEXT;
	RETURN;
END;
$$ LANGUAGE plpgsql;
