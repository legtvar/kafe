import { Box, Flex, FormControl, Highlight, Input, Text, useColorModeValue } from '@chakra-ui/react';
import { t } from 'i18next';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Playlist } from '../../../data/Playlist';
import { useColorScheme, useHighlightStyle } from '../../../hooks/useColorScheme';
import { fulltextFilter } from '../../../utils/fulltextFilter';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { Pagination } from '../../utils/Pagination';
import { useTitle } from '../../../utils/useTitle';

interface IPlaylistListProps {}

export function PlaylistList(props: IPlaylistListProps) {
    const borderColor = useColorModeValue('gray.300', 'gray.700');
    const hoverColor = useColorModeValue('gray.200', 'gray.700');
    useTitle(t("title.playlists"));

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
