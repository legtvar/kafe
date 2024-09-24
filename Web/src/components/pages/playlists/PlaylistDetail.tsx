import { Box, Flex, Icon, Stack, Text, useColorModeValue } from '@chakra-ui/react';
import { IoPlayOutline } from 'react-icons/io5';
import { Link, Navigate, useNavigate, useParams } from 'react-router-dom';
import { Playlist } from '../../../data/Playlist';
import { getPrefered } from '../../../utils/preferedLanguage';
import { ContentViewer } from '../../media/ContentViewer';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { Status } from '../../utils/Status';

interface IPlaylistDetailProps {}

export function PlaylistDetail(props: IPlaylistDetailProps) {
    const { id, itemId } = useParams();
    const borderColor = useColorModeValue('gray.300', 'gray.700');
    const hoverColor = useColorModeValue('gray.200', 'gray.700');
    const navigate = useNavigate();

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    if (itemId === undefined) {
        return <Navigate to="1" />;
    }

    const item = parseInt(itemId);

    if (!(item > 0)) {
        return <Navigate to="./1" />;
    }

    return (
        <AwaitAPI request={(api) => api.playlists.getById(id)} error={<Status statusCode={404} embeded />}>
            {(playlist: Playlist) => (
                <AwaitAPI
                    request={(api) => api.playlists.getById(playlist.id)}
                    error={<Status statusCode={404} embeded />}
                >
                    {(playlist: Playlist) => {
                        const artifacts = playlist.entries
                        return (
                            <Stack direction={{ base: 'column', lg: 'row' }} spacing={4}>
                                <Box width="full">
                                    {artifacts.length < item ? (
                                        <Navigate to="./1" />
                                    ) : (
                                        <AwaitAPI request={(api) => api.artifacts.getById(artifacts[item - 1].id)} key={artifacts[item - 1].id}>
                                            {(artifact) => (
                                                <ContentViewer
                                                    key={0}
                                                    artifact={artifact}
                                                    autoplay
                                                    videoProps={{
                                                        onEnded: () => {
                                                            if (item < artifacts.length) {
                                                                navigate(`./${item + 1}`);
                                                            }
                                                        },
                                                    }}
                                                    onNext={
                                                        item < artifacts.length ? () => navigate(`./${item + 1}`) : undefined
                                                    }
                                                    onPrevious={item > 1 ? () => navigate(`./${item - 1}`) : undefined}
                                                />
                                            )}
                                        </AwaitAPI>
                                    )}
                                </Box>
                                <Box borderWidth={1} w={{ base: '100%', lg: 'sm', xl: 'md' }} h="100%" py={3}>
                                    <Stack spacing={4} direction="column" overflowY="auto">
                                        <Box fontSize=" xl" fontWeight="semibold" as="h2" lineHeight="tight" px={4}>
                                            {playlist.getName()}
                                        </Box>
                                        <Box px={4}>{playlist.getDescription()}</Box>

                                        <Flex direction="column" w="full" mt={-4}>
                                            {artifacts.map((artifact, i) => (
                                                <Link to={`./${i + 1}`} key={i}>
                                                    <Flex
                                                        direction={{
                                                            base: 'column',
                                                            md: 'row',
                                                        }}
                                                        py={4}
                                                        px={4}
                                                        borderTopWidth="1px"
                                                        borderTopColor={borderColor}
                                                        align={'center'}
                                                        cursor="pointer"
                                                        _hover={{
                                                            background: hoverColor,
                                                        }}
                                                    >
                                                        <Flex direction="row" flex="1" alignItems="center">
                                                            {item === i + 1 && <Icon mr={2} as={IoPlayOutline} />}
                                                            <Text>{getPrefered(artifact.name)}</Text>
                                                        </Flex>
                                                    </Flex>
                                                </Link>
                                            ))}
                                        </Flex>
                                    </Stack>
                                </Box>
                            </Stack>
                        );
                    }}
                </AwaitAPI>
            )}
        </AwaitAPI>
    );
}
