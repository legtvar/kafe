import {
    Box,
    Button,
    Flex,
    HStack,
    Heading,
    Icon,
    IconButton,
    Tab,
    TabList,
    TabPanel,
    TabPanels,
    Tabs,
    Text,
    VStack,
    useDisclosure,
} from '@chakra-ui/react';
import { t } from 'i18next';
import { useCallback } from 'react';
import { BsArrowsVertical, BsX } from 'react-icons/bs';
import { IoAdd, IoSaveOutline, IoTrashOutline } from 'react-icons/io5';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { API } from '../../../api/API';
import { EntityPermissions } from '../../../data/EntityPermissions';
import { Playlist } from '../../../data/Playlist';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { getPrefered } from '../../../utils/preferedLanguage';
import { observeAbstactType } from '../../utils/AbstractTypeObserver';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { DraggableList } from '../../utils/DraggableList';
import { PermsEditor } from '../../utils/PermsEditor';
import { SendAPI } from '../../utils/SendAPI';
import { Status } from '../../utils/Status';
import { WithTitle } from '../../utils/WithTitle';
import { PlaylistAddNewFile } from './addNewFile/PlaylistAddNewFile';
import { PlaylistBasicInfo } from './PlaylistBasicInfo';

interface IPlaylistEditProps {}

export function PlaylistEdit(props: IPlaylistEditProps) {
    const { id } = useParams();
    const navigate = useNavigate();
    const { bg, border } = useColorScheme();
    const { isOpen, onClose, onOpen } = useDisclosure();

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    const getPerms = useCallback((api: API) => api.entities.perms.getById(id), [id]);

    return (
        <>
            <AwaitAPI
                request={useCallback((api) => api.playlists.getById(id), [id])}
                error={(resp) => {
                    return <Status statusCode={resp.response.status} log={resp.response.detail} embeded />;
                }}
            >
                {observeAbstactType((playlist: Playlist) => (
                    <AwaitAPI request={getPerms}>
                        {observeAbstactType((perms: EntityPermissions) => (
                            <Box m={6} pb={12}>
                                <WithTitle title={t('title.playlist', { playlist: playlist.getName() })} />
                                <Flex
                                    direction={{
                                        base: 'column',
                                        md: 'row',
                                    }}
                                    gap={4}
                                    mb={6}
                                    alignItems="start"
                                >
                                    <Heading fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" mr="auto">
                                        {playlist.getName()}
                                    </Heading>
                                    <SendAPI
                                        value={playlist}
                                        request={(api: API, value: Playlist) => api.playlists.update(value)}
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
                                                        leftIcon={<IoSaveOutline />}
                                                        colorScheme="blue"
                                                        isDisabled={
                                                            playlist.changed.size === 0 && perms.changed.size === 0
                                                        }
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
                                </Flex>
                                <Tabs>
                                    <TabList>
                                        <Tab>{t('playlistsEdit.tabs.info').toString()}</Tab>
                                        <Tab>{t('playlistsEdit.tabs.files').toString()}</Tab>
                                        <Tab>{t('playlistsEdit.tabs.perms').toString()}</Tab>
                                    </TabList>

                                    <TabPanels pt={6}>
                                        <TabPanel>
                                            <PlaylistBasicInfo playlist={playlist} noSelfSubmit />
                                        </TabPanel>
                                        <TabPanel>
                                            <Text fontStyle="italic">
                                                {t('playlistsEdit.files.description').toString()}
                                            </Text>
                                            <Box h={2} borderY="solid 1px" borderColor={border} my={6} />
                                            {(() => {
                                                console.log(playlist.entries.map((e) => e.id));
                                                return <></>;
                                            })()}
                                            <DraggableList
                                                order={playlist.entries.map((e) => e.id)}
                                                // handle=".sortable-handle"
                                                items={playlist.entries.reduce(
                                                    (acc, item) => ({
                                                        ...acc,
                                                        [item.id]: (
                                                            <AwaitAPI request={(api) => api.artifacts.getById(item.id)}>
                                                                {(artifact) => (
                                                                    <AwaitAPI
                                                                        request={(api) =>
                                                                            api.projects.getById(
                                                                                artifact.containingProjectIds[0],
                                                                            )
                                                                        }
                                                                    >
                                                                        {(project) => (
                                                                            <HStack
                                                                                userSelect="none"
                                                                                bg={bg}
                                                                                mb={2}
                                                                                py={4}
                                                                                px={4}
                                                                                borderRadius="md"
                                                                                borderColor={border}
                                                                                borderStyle="solid"
                                                                                borderWidth={1}
                                                                                cursor="grab"
                                                                                spacing={4}
                                                                            >
                                                                                <Icon
                                                                                    as={BsArrowsVertical}
                                                                                    opacity={0.5}
                                                                                    className="sortable-handle"
                                                                                />
                                                                                <HStack flexGrow={1}>
                                                                                    <Text fontWeight="bold">
                                                                                        {getPrefered(item.name)}
                                                                                    </Text>{' '}
                                                                                    <Text>({project.getName()})</Text>
                                                                                </HStack>
                                                                                <IconButton
                                                                                    className="hide-on-drag"
                                                                                    icon={<IoTrashOutline />}
                                                                                    aria-label="Remove from playlist"
                                                                                    colorScheme="red"
                                                                                    onClick={() =>
                                                                                        playlist.set(
                                                                                            'entries',
                                                                                            playlist.entries.filter(
                                                                                                (e) => e.id !== item.id,
                                                                                            ),
                                                                                        )
                                                                                    }
                                                                                />
                                                                            </HStack>
                                                                        )}
                                                                    </AwaitAPI>
                                                                )}
                                                            </AwaitAPI>
                                                        ),
                                                    }),
                                                    {} as Record<string, JSX.Element>,
                                                )}
                                                onChange={(order) => {
                                                    console.log(order, playlist.entries);

                                                    playlist.set(
                                                        'entries',
                                                        order.map((key: string) => {
                                                            const found = playlist.entries.find(
                                                                (entry) => entry.id === key,
                                                            )!;
                                                            console.log(found, key);
                                                            return found;
                                                        }),
                                                    );
                                                }}
                                            />
                                            <Box h={2} borderY="solid 1px" borderColor={border} my={6} />
                                            <Button
                                                borderRadius="md"
                                                colorScheme="blue"
                                                w="full"
                                                justifyContent="start"
                                                py={7}
                                                px={6}
                                                leftIcon={<IoAdd />}
                                                onClick={onOpen}
                                            >
                                                {t('playlistsEdit.addFile').toString()}
                                            </Button>
                                            <PlaylistAddNewFile
                                                isOpen={isOpen}
                                                onClose={(files) => {
                                                    if (files.length > 0) {
                                                        playlist.set('entries', [...playlist.entries, ...files]);
                                                    }
                                                    onClose();
                                                }}
                                            />
                                        </TabPanel>
                                        <TabPanel>
                                            <VStack align="stretch">
                                                <PermsEditor
                                                    perms={perms}
                                                    options={['read', 'write', 'inspect']}
                                                    explanation={{
                                                        read: t('perms.groups.playlist.read').toString(),
                                                        write: t('perms.groups.playlist.write').toString(),
                                                        inspect: t('perms.groups.playlist.inspect').toString(),
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
        </>
    );
}
