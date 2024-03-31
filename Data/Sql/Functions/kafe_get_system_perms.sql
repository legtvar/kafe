CREATE OR REPLACE FUNCTION {databaseSchema}.kafe_get_system_perms(
    IN accessing_account_id character varying
) RETURNS int AS $$
DECLARE
    accessing_account_perms jsonb;
BEGIN
    SET search_path TO {databaseSchema};

    IF accessing_account_id IS NULL THEN
        RETURN 0;
    END IF;

    accessing_account_perms := (SELECT data -> 'Permissions'
                                FROM mt_doc_accountinfo
                                WHERE data ->> 'Id' = accessing_account_id);

    RETURN (accessing_account_perms -> 'system')::int;
END;
$$ LANGUAGE plpgsql;
