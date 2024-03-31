CREATE OR REPLACE FUNCTION {databaseSchema}.kafe_get_playlist_perms_core(
	IN playlist jsonb,
	IN accessing_account jsonb
) RETURNS int AS $$
DECLARE
	perms int := 0;
BEGIN
	SET search_path TO {databaseSchema};

	IF playlist IS NULL THEN
		RAISE 'Playlist not found.';
	END IF;

	IF playlist -> 'GlobalPermissions' IS NOT NULL THEN
		perms := perms | (playlist -> 'GlobalPermissions')::int;
	END IF;
	
	IF accessing_account -> 'Permissions' -> 'system' IS NOT NULL THEN
		perms := perms | (accessing_account -> 'Permissions' -> 'system')::int;
	END IF;
	
	IF accessing_account -> 'Permissions' -> (playlist ->> 'Id') IS NOT NULL THEN
		perms := perms | (accessing_account -> 'Permissions' -> (playlist ->> 'Id'))::int;
	END IF;
	
	RETURN perms;
END;
$$ LANGUAGE plpgsql;
