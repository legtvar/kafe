import { Flex, Text, useColorModeValue } from '@chakra-ui/react';
import { Link } from 'react-router-dom';
import { Group } from '../../../data/Group';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { Pagination } from '../../utils/Pagination';

interface IGroupsProps {}

export function Groups(props: IGroupsProps) {
    const borderColor = useColorModeValue('gray.300', 'gray.700');
    const hoverColor = useColorModeValue('gray.200', 'gray.700');
    return (
        <>
            <AwaitAPI request={(api) => api.groups.getAll()}>
                {(data: Group[]) => (
                    <Flex direction="column" w="full" mt={-4}>
                        <Pagination data={data}>
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
                                    </Flex>
                                </Link>
                            )}
                        </Pagination>
                    </Flex>
                )}
            </AwaitAPI>
        </>
    );
}
