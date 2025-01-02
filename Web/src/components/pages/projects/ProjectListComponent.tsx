import { Box, Flex, FormControl, Highlight, Icon, Input, Text, useColorModeValue } from '@chakra-ui/react';
import { t } from 'i18next';
import moment from 'moment';
import { useState } from 'react';
import { IoCubeOutline } from 'react-icons/io5';
import { Link } from 'react-router-dom';
import { Project } from '../../../data/Project';
import { useAuthLink } from '../../../hooks/useAuthLink';
import { useColorScheme, useHighlightStyle } from '../../../hooks/useColorScheme';
import { fulltextFilter } from '../../../utils/fulltextFilter';
import { Pagination } from '../../utils/Pagination';

interface IProjectListComponentProps {
    projects: Project[];
}

export function ProjectListComponent({ projects }: IProjectListComponentProps) {
    const { border, bg } = useColorScheme();

    const borderColor = useColorModeValue('gray.300', 'gray.700');
    const hoverColor = useColorModeValue('gray.200', 'gray.700');

    const highlightStyle = useHighlightStyle();
    const [filter, setFilter] = useState('');

    return (
        <>
            <Flex direction="column" w="full">
                <Box mx={-4} px={4} pb={4} borderBottomWidth="1px" borderBottomColor={borderColor}>
                    <FormControl>
                        <Input
                            type="text"
                            borderColor={border}
                            bg={bg}
                            placeholder={`${t('projectList.search').toString()}`}
                            value={filter}
                            onChange={(event) => setFilter(event.target.value.toLowerCase())}
                        />
                    </FormControl>
                </Box>
                <Pagination
                    data={projects
                        .filter(
                            (project) =>
                                fulltextFilter(project.name, filter) || fulltextFilter(project.description, filter),
                        )
                        .sort((a, b) =>
                            a.releasedOn ? (b.releasedOn ? b.releasedOn.getTime() - a.releasedOn.getTime() : -1) : 1,
                        )}
                >
                    {(project, i) => (
                        <Link to={useAuthLink(`/projects/${project.id}`)} key={i}>
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
                                align={'start'}
                                cursor="pointer"
                                _hover={{
                                    background: hoverColor,
                                }}
                            >
                                <Icon as={IoCubeOutline} mb="auto" mr={3} my={1} fontSize="xl" />
                                <Flex direction="column" flex="1">
                                    <Text>
                                        {
                                            <Highlight styles={highlightStyle} query={filter}>
                                                {project.getName()}
                                            </Highlight>
                                        }
                                    </Text>
                                    <Text fontSize="smaller" color="gray.500">
                                        <Highlight styles={highlightStyle} query={filter}>
                                            {project.getDescription()}
                                        </Highlight>
                                    </Text>
                                </Flex>
                                <Text
                                    flex={{
                                        base: '1 0 0%',
                                        md: '0 0 120px',
                                    }}
                                    textAlign={{
                                        base: 'left',
                                        md: 'right',
                                    }}
                                    color="gray.500"
                                >
                                    {project.releasedOn ? moment(project.releasedOn).calendar() : 'Neuvedeno'}
                                </Text>
                            </Flex>
                        </Link>
                    )}
                </Pagination>
            </Flex>
        </>
    );
}
