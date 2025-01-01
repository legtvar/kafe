import { Center, Flex, Heading, HStack, Text, VStack } from '@chakra-ui/layout';
import { useForceUpdate } from '@chakra-ui/react';
import { t } from 'i18next';
import { Link, Navigate } from 'react-router-dom';
import { useOrganizations } from '../../../hooks/Caffeine';
import { useAuthLink } from '../../../hooks/useAuthLink';
import { Navbar } from '../../layout/navigation/Navbar';
import { OrganizationAvatar } from '../../utils/OrganizationAvatar/OrganizationAvatar';
import { OutletOrChildren } from '../../utils/OutletOrChildren';

export const LS_LATEST_ORG_KEY = 'kafe_latest_org';

export function OrganizationRedirect() {
    const org = localStorage.getItem(LS_LATEST_ORG_KEY);
    const { organizations } = useOrganizations();
    const reload = useForceUpdate();

    // if (organizations.length === 1) {
    //     localStorage.setItem(LS_LATEST_ORG_KEY, organizations[0].id);
    //     return <Navigate to={useAuthLink(undefined, organizations[0].id)} />;
    // }

    return (
        <OutletOrChildren>
            {org ? (
                <Navigate to={org} />
            ) : (
                <Flex direction="column" w="100vw" h="100vh" align="stretch">
                    <Navbar signedIn={true} forceReload={() => reload()} />
                    <Flex direction="row" flexGrow={1} align="stretch" minH={0} minW={0}>
                        <Center px={6} flexGrow={1} overflowY="auto">
                            <VStack maxW="full">
                                <Heading as="h1" textAlign="center" maxW="full">
                                    {t('organizationRedirect.title')}
                                </Heading>
                                <Text textAlign="center" maxW="full">
                                    {t('organizationRedirect.subtitle')}
                                </Text>
                                <HStack mt={8} spacing={6} overflowX="auto" minW={0} maxW="full">
                                    {organizations.map((org, key) => (
                                        <Link
                                            to={useAuthLink(undefined, org.id)}
                                            key={key}
                                            onClick={() => {
                                                localStorage.setItem(LS_LATEST_ORG_KEY, org.id);
                                            }}
                                        >
                                            <VStack key={key} w={32} flexGrow={0} flexShrink={0} spacing={6}>
                                                <OrganizationAvatar organization={org} size="xl" />
                                                <Text textAlign="center" noOfLines={1} fontSize="lg" fontWeight="bold">
                                                    {org.getName()}
                                                </Text>
                                            </VStack>
                                        </Link>
                                    ))}
                                </HStack>
                            </VStack>
                        </Center>
                    </Flex>
                </Flex>
            )}
        </OutletOrChildren>
    );
}
