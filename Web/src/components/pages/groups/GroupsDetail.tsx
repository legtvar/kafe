import { Box, Button, Flex, Heading, Spacer, Stack } from '@chakra-ui/react';
import ChakraUIRenderer from 'chakra-ui-markdown-renderer';
import { t } from 'i18next';
import { useCallback } from 'react';
import { AiOutlineEdit } from 'react-icons/ai';
import Markdown from 'react-markdown';
import { Link, useParams } from 'react-router-dom';
import { Group } from '../../../data/Group';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { Status } from '../../utils/Status';
import { WithTitle } from '../../utils/WithTitle';
import { ProjectListComponent } from '../projects/ProjectListComponent';

interface IGroupsDetailProps {}

export function GroupsDetail(props: IGroupsDetailProps) {
    const { id } = useParams();

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    return (
        <OutletOrChildren>
            <AwaitAPI
                request={useCallback((api) => api.groups.getById(id), [id])}
                error={(error) => <Status statusCode={404} embeded log={error} />}
            >
                {(group: Group) => (
                    <Stack spacing={4} m={6} direction="column" pb={16}>
                        <WithTitle title={t('title.group', { group: group.getName() })} />
                        <Flex mb={2}>
                            <Heading fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" mr="auto">
                                {group.getName()}
                            </Heading>
                            <Link to="edit">
                                <Button leftIcon={<AiOutlineEdit />}>{t('generic.edit').toString()}</Button>
                            </Link>
                        </Flex>
                        <Box>
                            <Markdown components={ChakraUIRenderer()} skipHtml>
                                {group.getDescription()}
                            </Markdown>
                        </Box>

                        {group.isOpen && (
                            <>
                                <Flex direction="row" pb={12}>
                                    <Link to="create">
                                        <Button colorScheme="brand">{t('createProject.signUp').toString()}</Button>
                                    </Link>
                                    <Spacer />
                                </Flex>
                            </>
                        )}
                        <ProjectListComponent projects={group.projects} />
                    </Stack>
                )}
            </AwaitAPI>
        </OutletOrChildren>
    );
}
