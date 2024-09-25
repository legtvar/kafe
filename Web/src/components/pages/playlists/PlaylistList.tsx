import { Box, Flex, FormControl, Highlight, Icon, Input, Text, useColorModeValue } from '@chakra-ui/react';
import { t } from 'i18next';
import { useState } from 'react';
import { IoListCircleOutline } from 'react-icons/io5';
import { Link } from 'react-router-dom';
import { Playlist } from '../../../data/Playlist';
import { useColorScheme, useHighlightStyle } from '../../../hooks/useColorScheme';
import { fulltextFilter } from '../../../utils/fulltextFilter';
import { useTitle } from '../../../utils/useTitle';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { Pagination } from '../../utils/Pagination';

interface IPlaylistListProps {}

export function PlaylistList(props: IPlaylistListProps) {
    const borderColor = useColorModeValue('gray.300', 'gray.700');
    const hoverColor = useColorModeValue('gray.200', 'gray.700');
    useTitle(t('title.playlists'));

    const { border, bg } = useColorScheme();
    const highlightStyle = useHighlightStyle();
    const [filter, setFilter] = useState('');

    return (
        <OutletOrChildren>
            <AwaitAPI request={(api) => api.playlists.getAll()}>
                {(data: Playlist[]) => (
                    <>
                        <Box mx={-4} px={4} pb={4} borderBottomWidth="1px" borderBottomColor={borderColor}>
                            <FormControl>
                                <Input
                                    type="text"
                                    borderColor={border}
                                    bg={bg}
                                    placeholder={`${t('playlistList.search').toString()}`}
                                    value={filter}
                                    onChange={(event) => setFilter(event.target.value.toLowerCase())}
                                />
                            </FormControl>
                        </Box>
                        <Flex direction="column" w="full">
                            <Pagination
                                data={data.filter(
                                    (playlist) =>
                                        fulltextFilter(playlist.name, filter) ||
                                        fulltextFilter(playlist.description, filter),
                                )}
                            >
                                {(project, i) => (
                                    <Link to={project.id} key={i}>
                                        <Flex
                                            direction={{
                                                base: 'column',
                                                md: 'row',
                                            }}
                                            mx={-4}
                                            py={7}
                                            px={8}
                                            borderBottomWidth="1px"
                                            borderBottomColor={borderColor}
                                            align={'center'}
                                            cursor="pointer"
                                            _hover={{
                                                background: hoverColor,
                                            }}
                                        >
                                            <Icon as={IoListCircleOutline} mb="auto" mr={3} mt={1} fontSize="xl" />
                                            <Flex direction="column" flex="1">
                                                <Text>
                                                    <Highlight styles={highlightStyle} query={filter}>
                                                        {project.getName()}
                                                    </Highlight>
                                                </Text>
                                                <Text fontSize="smaller" color="gray.500">
                                                    <Highlight styles={highlightStyle} query={filter}>
                                                        {project.getDescription()}
                                                    </Highlight>
                                                </Text>
                                            </Flex>
                                        </Flex>
                                    </Link>
                                )}
                            </Pagination>
                        </Flex>
                    </>
                )}
            </AwaitAPI>
        </OutletOrChildren>
    );
}
