import { Box, Button, Flex, Spacer, Stack } from '@chakra-ui/react';
import { Link, useParams } from 'react-router-dom';
import { Group } from '../../../data/Group';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { Status } from '../../utils/Status';

interface IGroupsDetailProps {}

export function GroupsDetail(props: IGroupsDetailProps) {
    const { id } = useParams();

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    return (
        <OutletOrChildren>
            <AwaitAPI request={(api) => api.groups.getById(id)} error={<Status statusCode={404} embeded />}>
                {(group: Group) => (
                    <Stack spacing={4} m={6} direction="column">
                        <Box fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" isTruncated>
                            {group.getName()}
                        </Box>
                        <Box>{group.getDescription()}</Box>
                        <Flex direction="row">
                            <Link to="create">
                                <Button>Přihlásit film</Button>
                            </Link>
                            <Spacer />
                        </Flex>
                    </Stack>
                )}
            </AwaitAPI>
        </OutletOrChildren>
    );
}
