import { AspectRatio, Box, Flex, Stack, Text, useColorModeValue } from '@chakra-ui/react';
import { BsPlayFill } from 'react-icons/bs';
import { useParams } from 'react-router-dom';
import { Playlist } from '../../../data/Playlist';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { Status } from '../../utils/Status';

interface IPlaylistDetailProps {}

export function PlaylistDetail(props: IPlaylistDetailProps) {
    const { id } = useParams();
    const borderColor = useColorModeValue('gray.300', 'gray.700');
    const hoverColor = useColorModeValue('gray.200', 'gray.700');

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    return (
        <AwaitAPI request={(api) => api.playlists.getById(id)} error={<Status statusCode={404} embeded />}>
            {(playlist: Playlist) => {
                console.log(playlist);
                return (
                    <Stack direction={{ base: 'column', lg: 'row' }} spacing={4}>
                        <AspectRatio ratio={16 / 9} w="100%" alignSelf="start">
                            <Box>
                                {/* <ContentViewer
                                    file={
                                        new FileEntry({
                                            name: 'Video',
                                            id: 'test',
                                            path: 'https://storage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4',
                                        })
                                    }
                                /> */}
                            </Box>
                        </AspectRatio>
                        <Box borderWidth={1} w={{ base: '100%', lg: 'sm', xl: 'md' }} h="100%" py={3}>
                            <Stack spacing={4} direction="column" overflowY="auto">
                                <Box fontSize=" xl" fontWeight="semibold" as="h2" lineHeight="tight" px={4}>
                                    {playlist.getName()}
                                </Box>
                                <Box px={4}>{playlist.getDescription()}</Box>

                                <Flex direction="column" w="full" mt={-4}>
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
                                        <Flex direction="column" flex="1">
                                            <Text>Test</Text>
                                        </Flex>
                                    </Flex>
                                    <Box
                                        py={4}
                                        px={4}
                                        borderTopWidth="1px"
                                        borderTopColor={borderColor}
                                        cursor="pointer"
                                        _hover={{
                                            background: hoverColor,
                                        }}
                                    >
                                        <BsPlayFill style={{ display: 'inline-block', lineHeight: '2em' }} /> Test
                                    </Box>
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
                                        <Flex direction="column" flex="1">
                                            <Text>Test</Text>
                                        </Flex>
                                    </Flex>
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
                                        <Flex direction="column" flex="1">
                                            <Text>Test</Text>
                                        </Flex>
                                    </Flex>
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
                                        <Flex direction="column" flex="1">
                                            <Text>Test</Text>
                                        </Flex>
                                    </Flex>
                                </Flex>
                            </Stack>
                        </Box>
                    </Stack>
                );
            }}
        </AwaitAPI>
    );
}
