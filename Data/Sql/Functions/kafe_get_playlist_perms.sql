CREATE OR REPLACE FUNCTION kafe_get_playlist_perms(
	IN playlist_id character varying,
	IN accessing_account_id character varying
) RETURNS int AS $$
DECLARE
	playlist jsonb;
	account jsonb;
BEGIN
	IF playlist_id IS NULL THEN
		RAISE '`playlist_id` must be non-null.';
	END IF;
	
	playlist := (SELECT data
				 FROM mt_doc_playlistinfo
				 WHERE data ->> 'Id' = playlist_id);
	
	account := (SELECT data
				FROM mt_doc_accountinfo
				WHERE data ->> 'Id' = accessing_account_id);
	
	RETURN kafe_get_playlist_perms_core(playlist, account);
END;
$$ LANGUAGE plpgsql;
