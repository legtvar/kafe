import { Box, Flex, Heading } from '@chakra-ui/react';
import { t } from 'i18next';
import { useCallback } from 'react';
import { useParams } from 'react-router-dom';
import { Author } from '../../../../data/Author';
import { AwaitAPI } from '../../../utils/AwaitAPI';
import { KafeAvatar } from '../../../utils/KafeAvatar';
import { OutletOrChildren } from '../../../utils/OutletOrChildren';
import { Status } from '../../../utils/Status';
import { WithTitle } from '../../../utils/WithTitle';

interface IAuthorDetailProps {}

export function AuthorDetail(props: IAuthorDetailProps) {
    const { id } = useParams();

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    return (
        <OutletOrChildren>
            <AwaitAPI
                request={useCallback((api) => api.authors.getById(id), [id])}
                error={(resp) => {
                    return <Status statusCode={resp.response.status} log={resp.response.detail} embeded />;
                }}
            >
                {(author: Author) => (
                    <Box m={6} pb={12}>
                        <WithTitle title={t('title.author', { author: author.name })} />
                        <Flex mb={2} align="center" gap={8}>
                            <KafeAvatar person={author} size="xl" />
                            <Heading fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" mr="auto">
                                {author.name}
                            </Heading>
                        </Flex>
                        <Box mt={6}>{author.getBio()}</Box>
                    </Box>
                )}
            </AwaitAPI>
        </OutletOrChildren>
    );
}
