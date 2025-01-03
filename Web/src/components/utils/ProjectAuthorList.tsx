import { CloseButton, HStack, SimpleGrid, Text, VStack } from '@chakra-ui/react';
import { useCallback } from 'react';
import { API } from '../../api/API';
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

function ProjectAuthorListAwaiter(
    props: Omit<IProjectAuthorListProps, 'authors'> & { author: components['schemas']['ProjectAuthorDto'] },
) {
    const request = useCallback((api: API) => api.authors.getById(props.author.id), [props.author]);
    return props.requestDetails ? (
        <AwaitAPI key={props.author.id} request={request}>
            {(data: Author) => <SimpleProjectAuthorList {...props} author={{ ...props.author, ...data }} />}
        </AwaitAPI>
    ) : (
        <SimpleProjectAuthorList {...props} author={props.author} />
    );
}

export function ProjectAuthorList(props: IProjectAuthorListProps) {
    return (
        <SimpleGrid alignItems="start" spacing={4} columns={{ base: 1, md: 2, lg: 3, xl: 4 }}>
            {props.authors.map((author, i) => (
                <ProjectAuthorListAwaiter {...props} author={author} key={i} />
            ))}
        </SimpleGrid>
    );
}

function SimpleProjectAuthorList({
    author,
    onRemove,
    editable,
}: { author: components['schemas']['ProjectAuthorDto'] } & Omit<IProjectAuthorListProps, 'authors'>) {
    return (
        // TODO: Uncomment when back-end is ready
        // <Link to={useAuthLink('/projects/authors/' + author.id)}>
        <HStack alignItems="start">
            <KafeAvatar size={'md'} person={author} />
            <VStack alignItems="start" pl={2} spacing={0}>
                <Text fontWeight="bolder">{author.name}</Text>
                <Text color="gray.500">{author.roles.join(', ')}</Text>
            </VStack>
            {editable && <CloseButton onClick={() => onRemove && onRemove(author.id)} />}
        </HStack>
        // </Link>
    );
}
