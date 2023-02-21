import { AspectRatio, Box, Button, Center, Image, SimpleGrid, Stack, Tag, useColorModeValue } from '@chakra-ui/react';
import { t } from 'i18next';
import { BsPlayFill } from 'react-icons/bs';
import { Link } from 'react-router-dom';
import { Playlist } from '../../../data/Playlist';
import { AwaitAPI } from '../../utils/AwaitAPI';

interface IPlaylistsProps {}

export function Playlists(props: IPlaylistsProps) {
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
                        <Stack direction="row" overflow="hidden" mb={16} w="100%" h={96} alignItems="center">
                            <Box h="100%" position="relative" role="group" overflow="hidden" rounded="md">
                                <Image
                                    src={`https://picsum.photos/seed/${first.id}/1920/1080`}
                                    objectPosition="center center"
                                    objectFit="cover"
                                    height="100%"
                                    rounded="md"
                                    __css={{
                                        aspectRatio: (16 / 9).toString(),
                                    }}
                                    _groupHover={{
                                        filter: 'blur(10px)',
                                        transform: 'scale(110%)',
                                    }}
                                    transition="ease 0.3s"
                                />
                                <Link to={`playlists/${first.id}`}>
                                    <Center
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
                                        fontSize={96}
                                    >
                                        <BsPlayFill />
                                    </Center>
                                </Link>
                            </Box>
                            <Stack py={6} px={8} spacing={2}>
                                <Box fontSize="xl" as="h2" lineHeight="tight" color="gray.500" isTruncated>
                                    {t('playlists.watchNewest').toString()}
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
                                <Stack direction="row" pt={6}>
                                    <Link to={`playlists/${first.id}`}>
                                        <Button colorScheme="brand" leftIcon={<BsPlayFill />}>
                                            {t('playlists.play').toString()}
                                        </Button>
                                    </Link>
                                    <Link to={`playlists/${first.id}`}>
                                        <Button colorScheme="gray">{t('playlists.readmore').toString()}</Button>
                                    </Link>
                                </Stack>
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
                                                src={`https://picsum.photos/seed/${playlist.id}/1920/1080`}
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
