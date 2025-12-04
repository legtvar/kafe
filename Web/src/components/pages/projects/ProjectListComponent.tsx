import { Box, Flex, FormControl, Highlight, Icon, Input, Text, useColorModeValue } from '@chakra-ui/react';
import { t } from 'i18next';
import { useState } from 'react';
import { IoCubeOutline } from 'react-icons/io5';
import { Link } from 'react-router-dom';
import { Project } from '../../../data/Project';
import { useAuthLink } from '../../../hooks/useAuthLink';
import { useColorScheme, useHighlightStyle } from '../../../hooks/useColorScheme';
import { fulltextFilter } from '../../../utils/fulltextFilter';
import { Pagination } from '../../utils/Pagination';
import { ProjectStatusMini } from './ProjectStatus';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { DateTime } from 'luxon';
import { User } from '../../../data/User';
import { useOrganizations } from '../../../hooks/Caffeine';

interface IProjectListComponentProps {
    projects: Project[];
}

export function ProjectListComponent({ projects }: IProjectListComponentProps) {
    const { border, bg } = useColorScheme();
    const { currentOrganization } = useOrganizations();

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
                                py={5}
                                px={5}
                                borderBottomWidth="1px"
                                borderBottomColor={borderColor}
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
                                    {project.ownerId && <AwaitAPI
                                        request={(api) => api.accounts.info.getById(project.ownerId!)}
                                        error={() => <></>}
                                    >
                                        {(owner: User) => {
                                            return (
                                                <Text color="gray.500" fontStyle="italic">
                                                    {owner.name && owner.name} {owner.uco && (<span>({owner.uco})</span>)}
                                                </Text>
                                            );
                                        }}
                                    </AwaitAPI>}
                                    <Text
                                        fontSize="smaller"
                                        color="gray.500"
                                        isTruncated
                                        maxWidth={{ base: '100%', md: '600px' }}
                                    >
                                        <Highlight styles={highlightStyle} query={filter}>
                                            {project.getDescription()}
                                        </Highlight>
                                    </Text>
                                </Flex>
                                <Flex
                                    justifyContent="end"
                                    alignItems="center"
                                    gap={4}
                                >
                                    <Text
                                        minW={100}
                                        color="gray.500"
                                    >
                                        {project.releasedOn
                                            ? DateTime.fromJSDate(project.releasedOn).toLocaleString()
                                            : t('project.releaseDate.na')}
                                    </Text>
                                    {project.userPermissions.includes('review') 
                                    && currentOrganization?.id === 'mate-fimuni' 
                                    && (<ProjectStatusMini project={project} stage={"pigeons-test"} />)}
                                </Flex>
                            </Flex>
                        </Link>
                    )}
                </Pagination>
            </Flex>
        </>
    );
}
