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

	IF (account -> 'Permissions' -> (project ->> 'ProjectGroupId'))::int & 4 = 4 THEN
		perms := perms | 2; -- read
	END IF;
	
	IF (account -> 'Permissions' -> (project ->> 'ProjectGroupId'))::int & 8 = 8 THEN
		perms := perms | 8; -- write
	END IF;

	IF (account -> 'Permissions' -> (project ->> 'ProjectGroupId'))::int & 16 = 16 THEN
		perms := perms | 16; -- review
	END IF;

	RETURN perms;
END
$$ LANGUAGE plpgsql;
