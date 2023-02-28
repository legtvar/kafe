import { Avatar, Flex, HStack, Text, useColorModeValue } from '@chakra-ui/react';
import moment from 'moment';
import { Link } from 'react-router-dom';
import { Project } from '../../../data/Project';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { Pagination } from '../../utils/Pagination';

interface IProjectsProps {}

export function Projects(props: IProjectsProps) {
    const borderColor = useColorModeValue('gray.300', 'gray.700');
    const hoverColor = useColorModeValue('gray.200', 'gray.700');

    return (
        <OutletOrChildren>
            <AwaitAPI request={(api) => api.projects.getAll()}>
                {(data: Project[]) => (
                    <Flex direction="column" w="full" mt={-4}>
                        <Pagination
                            data={data.sort((a, b) =>
                                a.releaseDate
                                    ? b.releaseDate
                                        ? b.releaseDate.getTime() - a.releaseDate.getTime()
                                        : -1
                                    : 1,
                            )}
                        >
                            {(project, i) => (
                                <Link to={project.id}>
                                    <Flex
                                        direction={{
                                            base: 'column',
                                            md: 'row',
                                        }}
                                        mx={-4}
                                        key={i}
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
                                        <HStack
                                            ml={{
                                                base: 0,
                                                md: 4,
                                            }}
                                            my={{
                                                base: 4,
                                                md: 0,
                                            }}
                                        >
                                            <Avatar
                                                size={'sm'}
                                                src={
                                                    'https://images.unsplash.com/photo-1619946794135-5bc917a27793?ixlib=rb-0.3.5&q=80&fm=jpg&crop=faces&fit=crop&h=200&w=200'
                                                }
                                            />
                                            <Avatar
                                                size={'sm'}
                                                src={
                                                    'https://images.unsplash.com/photo-1619946794135-5bc917a27793?ixlib=rb-0.3.5&q=80&fm=jpg&crop=faces&fit=crop&h=200&w=200'
                                                }
                                            />
                                            <Avatar
                                                size={'sm'}
                                                src={
                                                    'https://images.unsplash.com/photo-1619946794135-5bc917a27793?ixlib=rb-0.3.5&q=80&fm=jpg&crop=faces&fit=crop&h=200&w=200'
                                                }
                                            />
                                            <Avatar
                                                size={'sm'}
                                                src={
                                                    'https://images.unsplash.com/photo-1619946794135-5bc917a27793?ixlib=rb-0.3.5&q=80&fm=jpg&crop=faces&fit=crop&h=200&w=200'
                                                }
                                            />
                                        </HStack>
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
                                            {project.releaseDate ? moment(project.releaseDate).calendar() : 'Neuvedeno'}
                                        </Text>
                                    </Flex>
                                </Link>
                            )}
                        </Pagination>
                    </Flex>
                )}
            </AwaitAPI>
        </OutletOrChildren>
    );
}
