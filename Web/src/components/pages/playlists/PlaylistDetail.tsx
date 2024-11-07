import { Box, Button, Flex, Icon, Stack, Text, useColorModeValue, VStack } from '@chakra-ui/react';
import { t } from 'i18next';
import { AiOutlineEdit } from 'react-icons/ai';
import { IoPlayOutline } from 'react-icons/io5';
import { Link, Navigate, useNavigate, useOutlet, useParams } from 'react-router-dom';
import { ApiResponse } from '../../../api/API';
import { Playlist } from '../../../data/Playlist';
import { getPrefered } from '../../../utils/preferedLanguage';
import { ContentViewer } from '../../media/ContentViewer';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { AwaitMultiAPI } from '../../utils/AwaitMultiAPI';
import { Status } from '../../utils/Status';
import { WithTitle } from '../../utils/WithTitle';

interface IPlaylistDetailProps {}

export function PlaylistDetail(props: IPlaylistDetailProps) {
    const { id, itemId } = useParams();
    const borderColor = useColorModeValue('gray.300', 'gray.700');
    const hoverColor = useColorModeValue('gray.300', 'gray.600');
    const selectedColor = useColorModeValue('gray.200', 'gray.700');
    const navigate = useNavigate();

    const outlet = useOutlet();

    if (outlet) return outlet;

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    if (itemId === undefined) {
        return <Navigate to="1" replace />;
    }

    const item = parseInt(itemId);

    if (!(item > 0)) {
        return <Navigate to="../1" replace />;
    }

    return (
        <AwaitAPI request={(api) => api.playlists.getById(id)} error={<Status statusCode={404} embeded />}>
            {(playlist: Playlist) => {
                const artifacts = playlist.entries;

                if (item > artifacts.length) {
                    return <Navigate to="../1" replace />;
                }

                return (
                    <>
                        <WithTitle title={t('title.playlist', { playlist: playlist.getName() })} />
                        <AwaitMultiAPI
                            request={(api) => artifacts.map((artifact) => api.artifacts.getById(artifact.id))}
                        >
                            {(artifacts) => (
                                <AwaitMultiAPI
                                    request={(api) =>
                                        artifacts.map(async (artifact) => {
                                            const videoShard = artifact.shards.filter(
                                                (shard) => shard.kind === 'video',
                                            )[0];
                                            if (videoShard) {
                                                return api.shards.getById(videoShard.id);
                                            }
                                            return { data: null } as ApiResponse<null>;
                                        })
                                    }
                                >
                                    {(shards) => {
                                        console.log(shards);

                                        return (
                                            <Stack direction={{ base: 'column', lg: 'row' }} spacing={4}>
                                                <Stack direction="column" spacing={4} w={{ base: 'full', lg: 'full' }}>
                                                    <Box width="full">
                                                        {artifacts.length === 0 ? (
                                                            <Box></Box>
                                                        ) : artifacts.length < item ? (
                                                            <Navigate to="../1" />
                                                        ) : (
                                                            <ContentViewer
                                                                key={0}
                                                                artifact={artifacts[item - 1]}
                                                                autoplay
                                                                videoProps={{
                                                                    onEnded: () => {
                                                                        if (item < artifacts.length) {
                                                                            navigate(`../${item + 1}`);
                                                                        }
                                                                    },
                                                                }}
                                                                onNext={
                                                                    item < artifacts.length
                                                                        ? () => navigate(`../${item + 1}`)
                                                                        : undefined
                                                                }
                                                                onPrevious={
                                                                    item > 1
                                                                        ? () => navigate(`../${item - 1}`)
                                                                        : undefined
                                                                }
                                                            />
                                                        )}
                                                    </Box>
                                                    {artifacts.length > 0 && (
                                                        <VStack alignItems="start">
                                                            <Text fontSize="2xl" fontWeight="bold">
                                                                {getPrefered(artifacts[item - 1].name)}
                                                            </Text>
                                                            {artifacts[item - 1].containingProjectIds[0] && (
                                                                <AwaitAPI
                                                                    request={(api) =>
                                                                        api.projects.getById(
                                                                            artifacts[item - 1].containingProjectIds[0],
                                                                        )
                                                                    }
                                                                    key={artifacts[item - 1].containingProjectIds[0]}
                                                                    loader={<></>}
                                                                    error={<></>}
                                                                >
                                                                    {(project) => (
                                                                        <>
                                                                            <Text fontSize="md">
                                                                                {project.getName()}{' '}
                                                                                {project.genre &&
                                                                                    '(' + project.getGenre() + ')'}
                                                                            </Text>
                                                                            <Text opacity={0.5} fontStyle="italic">
                                                                                {project.getDescription()}
                                                                            </Text>
                                                                        </>
                                                                    )}
                                                                </AwaitAPI>
                                                            )}
                                                        </VStack>
                                                    )}
                                                </Stack>
                                                <Box
                                                    borderWidth={1}
                                                    w={{ base: '100%', lg: 'sm', xl: 'md' }}
                                                    flexShrink={0}
                                                    h="100%"
                                                    mb={4}
                                                >
                                                    <Stack
                                                        spacing={4}
                                                        direction="column"
                                                        overflowY="auto"
                                                        maxH={{ base: undefined, lg: 'calc(100vh - 120px)' }}
                                                    >
                                                        <Text
                                                            fontSize=" xl"
                                                            fontWeight="semibold"
                                                            as="h2"
                                                            lineHeight="tight"
                                                            px={4}
                                                            pt={4}
                                                            flexGrow={1}
                                                        >
                                                            {playlist.getName()}
                                                        </Text>
                                                        <Box px={4} opacity={0.5}>
                                                            {playlist.getDescription()}
                                                        </Box>

                                                        <Link to="../edit">
                                                            <Button leftIcon={<AiOutlineEdit />} mx={4}>
                                                                {t('generic.edit').toString()}
                                                            </Button>
                                                        </Link>

                                                        <Flex
                                                            direction="column"
                                                            w="full"
                                                            flexGrow={1}
                                                            flexShrink={0}
                                                            overflowY="auto"
                                                        >
                                                            {artifacts.length === 0 ? (
                                                                <Flex
                                                                    direction="row"
                                                                    py={4}
                                                                    px={4}
                                                                    borderTopWidth="1px"
                                                                    borderTopColor={borderColor}
                                                                    align={'center'}
                                                                    justify="center"
                                                                >
                                                                    <Text fontStyle="italic" opacity={0.5}>
                                                                        {t('playlists.empty')}
                                                                    </Text>
                                                                </Flex>
                                                            ) : (
                                                                artifacts.map((artifact, i) => (
                                                                    <Link to={`../${i + 1}`} key={i}>
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
                                                                            background={
                                                                                item === i + 1
                                                                                    ? selectedColor
                                                                                    : undefined
                                                                            }
                                                                            _hover={{
                                                                                background: hoverColor,
                                                                            }}
                                                                        >
                                                                            <Flex
                                                                                direction="row"
                                                                                flex="1"
                                                                                alignItems="center"
                                                                                w="full"
                                                                            >
                                                                                <Box w={6} flexShrink={0}>
                                                                                    {item === i + 1 && (
                                                                                        <Icon
                                                                                            mr={2}
                                                                                            as={IoPlayOutline}
                                                                                        />
                                                                                    )}
                                                                                </Box>
                                                                                <Box
                                                                                    flexGrow={1}
                                                                                    textOverflow="ellipsis"
                                                                                    overflowX="hidden"
                                                                                    whiteSpace="nowrap"
                                                                                >
                                                                                    {getPrefered(artifact.name)}
                                                                                </Box>
                                                                                <Box
                                                                                    w={20}
                                                                                    flexShrink={0}
                                                                                    flexGrow={0}
                                                                                    textAlign="end"
                                                                                >
                                                                                    {(
                                                                                        shards[i]?.variants as any
                                                                                    ).original?.duration?.split(
                                                                                        '.',
                                                                                    )[0] || 'unknown'}
                                                                                </Box>
                                                                            </Flex>
                                                                        </Flex>
                                                                    </Link>
                                                                ))
                                                            )}
                                                        </Flex>
                                                    </Stack>
                                                </Box>
                                            </Stack>
                                        );
                                    }}
                                </AwaitMultiAPI>
                            )}
                        </AwaitMultiAPI>
                    </>
                );
            }}
        </AwaitAPI>
    );
}
