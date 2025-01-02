import { Box, Button, Flex, Heading, Stack } from '@chakra-ui/react';
import { t } from 'i18next';
import { useCallback } from 'react';
import { AiOutlineEdit, AiOutlinePlus } from 'react-icons/ai';
import { Link, useParams } from 'react-router-dom';
import { Group } from '../../../data/Group';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { ChakraMarkdown } from '../../utils/ChakraMarkdown';
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
                        <Flex
                            direction={{
                                base: 'column',
                                md: 'row',
                            }}
                            gap={4}
                            mb={6}
                            alignItems="start"
                        >
                            <Heading fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" mr="auto">
                                {group.getName()}
                            </Heading>
                            {group.isOpen && (
                                <Link to="create">
                                    <Button leftIcon={<AiOutlinePlus />} colorScheme="brand">
                                        {t('createProject.signUp').toString()}
                                    </Button>
                                </Link>
                            )}
                            <Link to="edit">
                                <Button leftIcon={<AiOutlineEdit />}>{t('generic.edit').toString()}</Button>
                            </Link>
                        </Flex>
                        <Box>
                            <ChakraMarkdown>{group.getDescription()}</ChakraMarkdown>
                        </Box>
                        <ProjectListComponent projects={group.projects} />
                    </Stack>
                )}
            </AwaitAPI>
        </OutletOrChildren>
    );
}
