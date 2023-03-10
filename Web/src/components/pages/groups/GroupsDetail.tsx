import { Box, Button, Flex, Spacer, Stack } from '@chakra-ui/react';
import { t } from 'i18next';
import { Link, useParams } from 'react-router-dom';
import { Group } from '../../../data/Group';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { Status } from '../../utils/Status';
import { IntroText } from '../projects/create/IntroText';
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
                request={(api) => api.groups.getById(id)}
                error={(error) => <Status statusCode={404} embeded log={error} />}
            >
                {(group: Group) => (
                    <Stack spacing={4} m={6} direction="column" pb={16}>
                        <Box fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" isTruncated>
                            {group.getName()}
                        </Box>
                        <Box>{group.getDescription()}</Box>

                        {group.isOpen && (
                            <>
                                <IntroText groupName={group.getName()} />

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
