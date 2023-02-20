import { AspectRatio, Box, Image, SimpleGrid, Stack, Tag, useColorModeValue } from '@chakra-ui/react';
import { t } from 'i18next';
import { Link } from 'react-router-dom';
import { Playlist } from '../../../data/Playlist';
import { AwaitAPI } from '../../utils/AwaitAPI';

interface IPublicPlaylistsProps {}

export function PublicPlaylists(props: IPublicPlaylistsProps) {
    const boxColor = useColorModeValue('white', 'gray.800');
    const overlayColor = useColorModeValue('whiteAlpha.800', 'grayAlpha.800');
    const tagColor = useColorModeValue('grayAlpha.200', 'whiteAlpha.200');

    return (
        <AwaitAPI request={(api) => api.playlists.getAll()}>
            {(data: Playlist[]) => {
                if (data.length === 0) return <></>;

                const first = data[0];
                const regular = data.slice(1);

                return (
                    <>
                        <Stack direction="row" role="group" overflow="hidden" mb={16} w="100%" h={96}>
                            <Box h="100%">
                                <Image
                                    src={
                                        'https://images.unsplash.com/photo-1572635196237-14b3f281503f?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=4600&q=80'
                                    }
                                    objectPosition="center center"
                                    objectFit="cover"
                                    height="100%"
                                    rounded="md"
                                    __css={{
                                        aspectRatio: (16 / 9).toString(),
                                    }}
                                />
                            </Box>
                            <Stack py={6} px={8} spacing={2}>
                                <Box fontSize="xl" as="h2" lineHeight="tight" color="gray.500" isTruncated>
                                    Podívejte se na nejnovější playlist
                                </Box>
                                <Box fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" isTruncated>
                                    {first.getName()}
                                </Box>
                                <Stack direction="row" spacing={2}>
                                    <Tag bg={tagColor}>16 {t('playlists.videoCount').toString()}</Tag>
                                    <Tag bg={tagColor}>2:34:56</Tag>
                                </Stack>
                                <Box lineHeight="tight" overflow="hidden">
                                    {first.getDescription()}
                                </Box>
                            </Stack>
                        </Stack>
                        <SimpleGrid columns={{ base: 1, md: 2, lg: 3, xl: 4 }} spacing={4} pb={4}>
                            {regular.map((playlist) => (
                                <Link to={`playlists/${playlist.id}`}>
                                    <AspectRatio ratio={16 / 9}>
                                        <Box
                                            bg={boxColor}
                                            borderWidth="1px"
                                            rounded="md"
                                            shadow="lg"
                                            flexShrink="0"
                                            display="inline-block"
                                            position="relative"
                                            role="group"
                                            overflow="hidden"
                                        >
                                            <Image
                                                src={
                                                    'https://images.unsplash.com/photo-1572635196237-14b3f281503f?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=4600&q=80'
                                                }
                                                objectPosition="center center"
                                                objectFit="cover"
                                                position="absolute"
                                                inset="0"
                                                height="100%"
                                                width="100%"
                                                _groupHover={{
                                                    filter: 'blur(10px)',
                                                    transform: 'scale(110%)',
                                                }}
                                                transition="ease 0.3s"
                                            />
                                            <Stack
                                                position="absolute"
                                                inset="0"
                                                height="100%"
                                                width="100%"
                                                background={overlayColor}
                                                opacity="0"
                                                _groupHover={{
                                                    opacity: '1',
                                                }}
                                                transition="ease 0.3s"
                                                py={6}
                                                px={8}
                                                spacing={2}
                                            >
                                                <Box
                                                    fontSize="2xl"
                                                    fontWeight="semibold"
                                                    as="h4"
                                                    lineHeight="tight"
                                                    isTruncated
                                                >
                                                    {playlist.getName()}
                                                </Box>
                                                <Stack direction="row" spacing={2}>
                                                    <Tag bg={tagColor}>16 {t('playlists.videoCount').toString()}</Tag>
                                                    <Tag bg={tagColor}>2:34:56</Tag>
                                                </Stack>
                                                <Box lineHeight="tight" overflow="hidden">
                                                    {playlist.getDescription()}
                                                </Box>
                                            </Stack>
                                        </Box>
                                    </AspectRatio>
                                </Link>
                            ))}
                        </SimpleGrid>
                    </>
                );
            }}
        </AwaitAPI>
    );
}
