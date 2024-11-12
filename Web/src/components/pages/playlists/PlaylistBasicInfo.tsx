import { Button, FormControl, FormLabel, HStack, Input, Stack, useConst, useForceUpdate } from '@chakra-ui/react';
import { t } from 'i18next';
import { useNavigate } from 'react-router-dom';
import { API } from '../../../api/API';
import { Playlist } from '../../../data/Playlist';
import { useAuthLinkFunction } from '../../../hooks/useAuthLink';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { HRIB } from '../../../schemas/generic';
import { LocalizedInput } from '../../utils/LocalizedInput';
import { SendAPI } from '../../utils/SendAPI';
import { TextareaMarkdown } from '../../utils/TextareaMarkdown';

interface IGroupBasicInfoProps {
    // Cannot be changed after initial draw
    playlist?: Playlist;
    noSelfSubmit?: boolean;
}

export function PlaylistBasicInfo(props: IGroupBasicInfoProps) {
    const { border, bg } = useColorScheme();
    const update = !!props.playlist;
    const playlist = useConst(props.playlist || new Playlist({} as any));
    const fu = useForceUpdate();
    const navigate = useNavigate();
    const authLink = useAuthLinkFunction();

    const forceUpdate = (any: any) => {
        fu();
    };

    const sendApiProps = update
        ? {
              onSubmited: (id: HRIB) => {
                  if (update) navigate(0);
                  else navigate(authLink(`/playlists/${id}/edit`));
              },
              value: playlist!,
              request: (api: API, value: Playlist) => api.playlists.update(value),
          }
        : {
              onSubmited: (id: HRIB) => {
                  navigate(authLink(`/playlists/${id}`));
              },
              value: playlist!,
              request: (api: API, value: Playlist) => api.playlists.create(value),
          };

    return (
        <SendAPI {...sendApiProps}>
            {(onSubmit, status) => (
                <Stack spacing={8} direction="column" mb={8}>
                    <FormControl>
                        <FormLabel>{t('reatePlaylist.fields.name').toString()}</FormLabel>
                        <LocalizedInput
                            as={Input}
                            type="text"
                            borderColor={border}
                            bg={bg}
                            name="name"
                            placeholder={t('reatePlaylist.fields.name').toString()}
                            value={playlist.name}
                            onChange={(value) => forceUpdate(playlist.set('name', value))}
                        />
                    </FormControl>

                    <FormControl>
                        <FormLabel>{t('reatePlaylist.fields.description').toString()}</FormLabel>
                        <LocalizedInput
                            as={TextareaMarkdown}
                            borderColor={border}
                            bg={bg}
                            name="description"
                            placeholder={t('reatePlaylist.fields.description').toString()}
                            value={playlist.description}
                            onChange={(value) => forceUpdate(playlist.set('description', value))}
                        />
                    </FormControl>

                    {!props.noSelfSubmit && (
                        <HStack w="100%">
                            <Button
                                colorScheme="blue"
                                ml="auto"
                                isDisabled={status === 'sending' || status === 'ok'}
                                onClick={onSubmit}
                            >
                                {update
                                    ? t('createGroup.button.update').toString()
                                    : t('createGroup.button.create').toString()}
                            </Button>
                        </HStack>
                    )}
                </Stack>
            )}
        </SendAPI>
    );
}
