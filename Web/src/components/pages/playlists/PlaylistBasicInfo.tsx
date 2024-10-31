import { Button, FormControl, FormLabel, HStack, Input, Stack, useConst, useForceUpdate } from '@chakra-ui/react';
import { t } from 'i18next';
import { useNavigate } from 'react-router-dom';
import { API } from '../../../api/API';
import { Playlist } from '../../../data/Playlist';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { HRIB } from '../../../schemas/generic';
import { getPrefered } from '../../../utils/preferedLanguage';
import { SendAPI } from '../../utils/SendAPI';
import { TextareaMarkdown } from '../../utils/TextareaMarkdown';

interface IGroupBasicInfoProps {
    // Cannot be changed after initial draw
    playlist?: Playlist;
    onChange?: (playlist: Playlist) => void;
    noSelfSubmit?: boolean;
}

export function PlaylistBasicInfo(props: IGroupBasicInfoProps) {
    const { border, bg } = useColorScheme();
    const update = !!props.playlist;
    const playlist = useConst(props.playlist || new Playlist({} as any));
    const fu = useForceUpdate();
    const navigate = useNavigate();

    const forceUpdate = (any: any) => {
        props.onChange?.(any);
        fu();
    };

    const sendApiProps = update
        ? {
              onSubmited: (id: HRIB) => {
                  if (update) navigate(0);
                  else navigate(`/auth/playlists/${id}/edit`);
              },
              value: playlist!,
              request: (api: API, value: Playlist) => api.playlists.update(value),
          }
        : {
              onSubmited: (id: HRIB) => {
                  navigate(`/auth/playlists/${id}`);
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
                        <Stack direction={{ base: 'column', md: 'row' }}>
                            <FormControl id="name.cs">
                                <Input
                                    type="text"
                                    borderColor={border}
                                    bg={bg}
                                    placeholder={`${t('reatePlaylist.fields.name').toString()} ${t(
                                        'createProject.language.cs',
                                    )}`}
                                    defaultValue={getPrefered(playlist.name, 'cs')}
                                    onChange={(event) =>
                                        forceUpdate(
                                            playlist.set('name', {
                                                ...playlist.name,
                                                cs: event.target.value,
                                            }),
                                        )
                                    }
                                />
                            </FormControl>

                            <FormControl id="name.en">
                                <Input
                                    type="text"
                                    borderColor={border}
                                    bg={bg}
                                    placeholder={`${t('reatePlaylist.fields.name').toString()} ${t(
                                        'createProject.language.en',
                                    )}`}
                                    defaultValue={getPrefered(playlist.name, 'en')}
                                    onChange={(event) =>
                                        forceUpdate(
                                            playlist.set('name', {
                                                ...playlist.name,
                                                en: event.target.value,
                                            }),
                                        )
                                    }
                                />
                            </FormControl>
                        </Stack>
                    </FormControl>

                    <FormControl>
                        <FormLabel>{t('reatePlaylist.fields.description').toString()}</FormLabel>
                        <Stack direction={{ base: 'column', md: 'row' }}>
                            <FormControl id="description.cs">
                                <TextareaMarkdown
                                    placeholder={`${t('reatePlaylist.fields.description').toString()} ${t(
                                        'createProject.language.cs',
                                    )}`}
                                    borderColor={border}
                                    bg={bg}
                                    defaultValue={getPrefered(playlist.description, 'cs')}
                                    onChange={(event) =>
                                        forceUpdate(
                                            playlist.set('description', {
                                                ...playlist.description,
                                                cs: event.target.value,
                                            }),
                                        )
                                    }
                                />
                            </FormControl>

                            <FormControl id="description.en">
                                <TextareaMarkdown
                                    placeholder={`${t('reatePlaylist.fields.description').toString()} ${t(
                                        'createProject.language.en',
                                    )}`}
                                    borderColor={border}
                                    bg={bg}
                                    defaultValue={getPrefered(playlist.description, 'en')}
                                    onChange={(event) =>
                                        forceUpdate(
                                            playlist.set('description', {
                                                ...playlist.description,
                                                en: event.target.value,
                                            }),
                                        )
                                    }
                                />
                            </FormControl>
                        </Stack>
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
