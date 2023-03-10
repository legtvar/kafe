import { Flex, Text, useColorModeValue } from '@chakra-ui/react';
import moment from 'moment';
import { Link } from 'react-router-dom';
import { Project } from '../../../data/Project';
import { Pagination } from '../../utils/Pagination';

interface IProjectListComponentProps {
    projects: Project[];
}

export function ProjectListComponent({ projects }: IProjectListComponentProps) {
    const borderColor = useColorModeValue('gray.300', 'gray.700');
    const hoverColor = useColorModeValue('gray.200', 'gray.700');

    return (
        <Flex direction="column" w="full" mt={-4}>
            <Pagination
                data={projects.sort((a, b) =>
                    a.releasedOn ? (b.releasedOn ? b.releasedOn.getTime() - a.releasedOn.getTime() : -1) : 1,
                )}
            >
                {(project, i) => (
                    <Link to={`/auth/projects/${project.id}`} key={i}>
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
                                <Text>{project.getName()}</Text>
                                <Text fontSize="smaller" color="gray.500">
                                    {project.getDescription()}
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
    );
}
