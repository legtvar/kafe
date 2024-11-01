import { CloseButton, HStack, SimpleGrid, Text, VStack } from '@chakra-ui/react';
import { Author } from '../../data/Author';
import { components } from '../../schemas/api';
import { HRIB } from '../../schemas/generic';
import { AwaitAPI } from './AwaitAPI';
import { KafeAvatar } from './KafeAvatar';

interface IProjectAuthorListProps {
    authors: components['schemas']['ProjectAuthorDto'][];
    editable?: boolean;
    requestDetails?: boolean;
    onRemove?: (id: HRIB) => void;
}

export function ProjectAuthorList(props: IProjectAuthorListProps) {
    return (
        <SimpleGrid alignItems="start" spacing={4} columns={{ base: 1, md: 2, lg: 3, xl: 4 }}>
            {props.authors.map((author, i) =>
                props.requestDetails ? (
                    <AwaitAPI key={author.id} request={(api) => api.authors.getById(author.id)}>
                        {(data: Author) => (
                            <SimpleProjectAuthorList key={i} {...props} author={{ ...author, ...data }} />
                        )}
                    </AwaitAPI>
                ) : (
                    <SimpleProjectAuthorList key={i} {...props} author={author} />
                ),
            )}
        </SimpleGrid>
    );
}

function SimpleProjectAuthorList({
    author,
    onRemove,
    editable,
}: { author: components['schemas']['ProjectAuthorDto'] } & IProjectAuthorListProps) {
    return (
        <HStack alignItems="start">
            <KafeAvatar size={'md'} person={author} />
            <VStack alignItems="start" pl={2} spacing={0}>
                <Text fontWeight="bolder">{author.name}</Text>
                <Text color="gray.500">{author.roles.join(', ')}</Text>
            </VStack>
            {editable && <CloseButton onClick={() => onRemove && onRemove(author.id)} />}
        </HStack>
    );
}
