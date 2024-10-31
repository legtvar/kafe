import { Box, Button, HStack, Heading, Tab, TabList, TabPanel, TabPanels, Tabs, Text, VStack } from '@chakra-ui/react';
import { t } from 'i18next';
import { BsSave, BsX } from 'react-icons/bs';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { API } from '../../../api/API';
import { EntityPermissions } from '../../../data/EntityPermissions';
import { Playlist } from '../../../data/Playlist';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { getPrefered } from '../../../utils/preferedLanguage';
import { AbstractTypeObserver, observeAbstactType } from '../../utils/AbstractTypeObserver';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { DraggableList } from '../../utils/DraggableList';
import { RightsEditor } from '../../utils/RightsEditor';
import { SendAPI } from '../../utils/SendAPI';
import { Status } from '../../utils/Status';
import { PlaylistBasicInfo } from './PlaylistBasicInfo';

interface IPlaylistEditProps {}

export function PlaylistEdit(props: IPlaylistEditProps) {
    const { id } = useParams();
    const navigate = useNavigate();
    const { bg, border } = useColorScheme();

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    return (
        <AwaitAPI request={(api) => api.playlists.getById(id)} error={<Status statusCode={404} embeded />}>
            {observeAbstactType((playlist: Playlist) => (
                <AwaitAPI request={(api) => api.entities.perms.getById(playlist.id)}>
                    {observeAbstactType((perms: EntityPermissions) => (
                        <Box m={6} pb={12}>
                            <HStack mb={2}>
                                <Heading fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" mr="auto">
                                    {playlist.getName()}
                                </Heading>
                                <SendAPI
                                    value={playlist}
                                    request={(api: API, value: Playlist) => api.playlists.create(value)}
                                    onSubmited={() => navigate(0)}
                                    repeatable={true}
                                >
                                    {(savePlaylist, playlistSaveStatus) => (
                                        <SendAPI
                                            value={perms}
                                            request={(api, value) => api.entities.perms.update(value)}
                                            onSubmited={() => navigate(0)}
                                            repeatable={true}
                                        >
                                            {(savePerms, permsSaveStatus) => (
                                                <Button
                                                    leftIcon={<BsSave />}
                                                    colorScheme="blue"
                                                    isDisabled={playlist.changed.size === 0 && perms.changed.size === 0}
                                                    onClick={() => {
                                                        if (playlist.changed.size > 0) {
                                                            savePlaylist();
                                                        }
                                                        if (perms.changed.size > 0) {
                                                            savePerms();
                                                        }
                                                    }}
                                                >
                                                    {t('generic.save').toString()}
                                                </Button>
                                            )}
                                        </SendAPI>
                                    )}
                                </SendAPI>
                                <Link to="..">
                                    <Button leftIcon={<BsX />}>{t('generic.quitEdit').toString()}</Button>
                                </Link>
                            </HStack>
                            <Tabs>
                                <TabList>
                                    <Tab>{t('playlistsEdit.tabs.info').toString()}</Tab>
                                    <Tab>{t('playlistsEdit.tabs.files').toString()}</Tab>
                                    <Tab>{t('playlistsEdit.tabs.rights').toString()}</Tab>
                                </TabList>

                                <TabPanels pt={6}>
                                    <TabPanel>
                                        <AbstractTypeObserver item={playlist}>
                                            {(playlist) => <PlaylistBasicInfo playlist={playlist} noSelfSubmit />}
                                        </AbstractTypeObserver>
                                    </TabPanel>
                                    <TabPanel>
                                        <DraggableList
                                            order={playlist.entries.map((e) => e.id)}
                                            items={playlist.entries.reduce(
                                                (acc, item) => ({
                                                    ...acc,
                                                    [item.id]: (
                                                        <HStack
                                                            bg={bg}
                                                            mb={2}
                                                            py={4}
                                                            px={6}
                                                            borderRadius="md"
                                                            borderColor={border}
                                                            borderStyle="solid"
                                                            borderWidth={1}
                                                            cursor="grab"
                                                        >
                                                            <Text fontWeight="bold">{getPrefered(item.name)}</Text>
                                                        </HStack>
                                                    ),
                                                }),
                                                {} as Record<string, JSX.Element>,
                                            )}
                                            onChange={(order) =>
                                                playlist.set(
                                                    'entries',
                                                    order.map(
                                                        (key: string) =>
                                                            playlist.entries.find((entry) => entry.id === key)!,
                                                    ),
                                                )
                                            }
                                        />
                                    </TabPanel>
                                    <TabPanel>
                                        <VStack align="stretch">
                                            <RightsEditor
                                                perms={perms}
                                                options={['read', 'write', 'inspect']}
                                                explanation={{
                                                    read: t('rights.groups.playlist.read').toString(),
                                                    write: t('rights.groups.playlist.write').toString(),
                                                    inspect: t('rights.groups.playlist.inspect').toString(),
                                                }}
                                            />
                                        </VStack>
                                    </TabPanel>
                                </TabPanels>
                            </Tabs>
                        </Box>
                    ))}
                </AwaitAPI>
            ))}
        </AwaitAPI>
    );
}
