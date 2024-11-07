import { Box, Heading, Stack } from '@chakra-ui/react';
import { t } from 'i18next';
import { useTitle } from '../../../utils/useTitle';
import { OrganizationBasicInfo } from './OrganizationBasicInfo';

interface IOrganizationsCreateProps {}

export function OrganizationsCreate(props: IOrganizationsCreateProps) {
    useTitle(t('title.organizationsCreate'));
    return (
        <Stack spacing={4} m={6} direction="column" pb={16}>
            <Box mb={2}>
                <Heading fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" mr="auto">
                    {t('title.organizationsCreate').toString()}
                </Heading>
            </Box>
            <OrganizationBasicInfo />
        </Stack>
    );
}
