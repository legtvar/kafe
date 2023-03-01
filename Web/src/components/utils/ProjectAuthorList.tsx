import { Avatar, HStack, SimpleGrid, Text, VStack } from '@chakra-ui/react';
import { components } from '../../schemas/api';
import { avatarUrl } from '../../utils/avatarUrl';

interface IProjectAuthorListProps {
    authors: components['schemas']['ProjectAuthorDto'][];
}

export function ProjectAuthorList(props: IProjectAuthorListProps) {
    return (
        <SimpleGrid alignItems="start" spacing={4} columns={{ base: 1, md: 2, lg: 3, xl: 4 }}>
            {props.authors.map((author) => (
                <HStack alignItems="start">
                    <Avatar size={'md'} src={avatarUrl(author.id)} />
                    <VStack alignItems="start" pl={2} spacing={0}>
                        <Text fontWeight="bolder">{author.name}</Text>
                        <Text color="gray.500">{author.roles.join(', ')}</Text>
                    </VStack>
                </HStack>
            ))}
        </SimpleGrid>
    );
}
