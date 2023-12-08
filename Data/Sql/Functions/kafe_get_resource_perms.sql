CREATE OR REPLACE FUNCTION kafe_get_resource_perms(
	IN resource_id character varying,
	IN account_id character varying
) RETURNS int AS $$
DECLARE
	resource_type character varying;
BEGIN
	IF resource_id IS NULL THEN
		RAISE '`resource_id` must be non-null.';
	END IF;
	
	resource_type := (SELECT type
					  FROM mt_streams
					  WHERE id = resource_id);
	IF resource_type IS NULL THEN
		RAISE 'The type of the resource could not be determined.';
	END IF;
	
	CASE
		WHEN resource_type = 'account_info' THEN
			RETURN kafe_get_account_perms(resource_id, account_id);
		WHEN resource_type = 'artifact_info' THEN
			RETURN kafe_get_artifact_perms(resource_id, account_id);
		WHEN resource_type = 'author_info' THEN
			RETURN kafe_get_author_perms(resource_id, account_id);
		WHEN resource_type = 'project_info' THEN
			RETURN kafe_get_project_perms(resource_id, account_id);
		WHEN resource_type = 'project_group_info' THEN
			RETURN kafe_get_projectgroup_perms(resource_id, account_id);
		ELSE
			RETURN FALSE;
	END CASE;
END;
$$ LANGUAGE plpgsql;
