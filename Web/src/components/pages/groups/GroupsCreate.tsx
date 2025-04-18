import { Box, Heading, Stack } from '@chakra-ui/react';
import { t } from 'i18next';
import { GroupBasicInfo } from './GroupBasicInfo';
import { useTitle } from '../../../utils/useTitle';

interface IGroupsCreateProps {}

export function GroupsCreate(props: IGroupsCreateProps) {
    useTitle(t("title.groupCreate"));
    return (
        <Stack spacing={4} m={6} direction="column" pb={16}>
            <Box mb={2}>
                <Heading fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" mr="auto">
                    {t('groupsCreate.title').toString()}
                </Heading>
            </Box>
            <GroupBasicInfo />
        </Stack>
    );
}
