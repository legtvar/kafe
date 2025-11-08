import { Box, Button, Flex, Heading, Tab, TabList, TabPanel, TabPanels, Tabs, VStack } from '@chakra-ui/react';
import { t } from 'i18next';
import { useCallback } from 'react';
import { BsX } from 'react-icons/bs';
import { IoSaveOutline } from 'react-icons/io5';
import { Link, useNavigate } from 'react-router-dom';
import { API } from '../../../api/API';
import { EntityPermissions } from '../../../data/EntityPermissions';
import { Organization } from '../../../data/Organization';
import { useOrganizations } from '../../../hooks/Caffeine';
import { observeAbstactType } from '../../utils/AbstractTypeObserver';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { PermsEditor } from '../../utils/PermsEditor';
import { SendAPI } from '../../utils/SendAPI';
import { Status } from '../../utils/Status';
import { WithTitle } from '../../utils/WithTitle';
import { OrganizationBasicInfo } from './OrganizationBasicInfo';

interface IOrganizationsEditProps {}

export function OrganizationsEdit(props: IOrganizationsEditProps) {
    const { currentOrganization } = useOrganizations();
    const id = currentOrganization!.id;
    const navigate = useNavigate();

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    const getPerms = useCallback((api: API) => api.entities.perms.getById(id), [id]);

    return (
        <AwaitAPI
            request={useCallback((api) => api.organizations.getById(id), [id])}
            error={(resp) => {
                return <Status statusCode={resp.response.status} log={resp.response.detail} embeded />;
            }}
        >
            {observeAbstactType((organization: Organization) => (
                <AwaitAPI request={getPerms}>
                    {observeAbstactType((perms: EntityPermissions) => (
                        <Box m={6} pb={12}>
                            <WithTitle title={t('title.organization', { group: organization.getName() })} />
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
                                    {organization.getName()}
                                </Heading>
                                <SendAPI
                                    value={organization}
                                    request={(api: API, value: Organization) => api.organizations.update(value)}
                                    onSubmited={() => navigate(0)}
                                    repeatable={true}
                                >
                                    {(saveOrganization, organizationSaveStatus) => (
                                        <SendAPI
                                            value={perms}
                                            request={(api, value) => api.entities.perms.update(value)}
                                            onSubmited={() => navigate(0)}
                                            repeatable={true}
                                        >
                                            {(savePerms, permsSaveStatus) => (
                                                <Button
                                                    leftIcon={<IoSaveOutline />}
                                                    colorScheme="blue"
                                                    isDisabled={
                                                        organization.changed.size === 0 && perms.changed.size === 0
                                                    }
                                                    onClick={() => {
                                                        if (organization.changed.size > 0) {
                                                            saveOrganization();
                                                        }
                                                        if (perms.changed.size > 0) {
                                                            savePerms();
                                                        }
                                                    }}
                                                >
                                                    {t('generic.save').toString()}
                                                </Button>
                                            )}
                                        </SendAPI>
                                    )}
                                </SendAPI>
                                <Link to="..">
                                    <Button leftIcon={<BsX />}>{t('generic.quitEdit').toString()}</Button>
                                </Link>
                            </Flex>
                            <Tabs>
                                <TabList>
                                    <Tab>{t('groupsEdit.tabs.info').toString()}</Tab>
                                    <Tab>{t('groupsEdit.tabs.perms').toString()}</Tab>
                                </TabList>

                                <TabPanels pt={6}>
                                    <TabPanel>
                                        <OrganizationBasicInfo organization={organization} noSelfSubmit />
                                    </TabPanel>
                                    <TabPanel>
                                        <VStack align="stretch">
                                            <PermsEditor
                                                perms={perms}
                                                options={['read', 'write', 'inspect', 'append', 'review']}
                                                explanation={{
                                                    read: t('perms.groups.organization.read').toString(),
                                                    write: t('perms.groups.organization.write').toString(),
                                                    inspect: t('perms.groups.organization.inspect').toString(),
                                                    append: t('perms.groups.organization.append').toString(),
                                                    review: t('perms.groups.organization.review').toString(),
                                                }}
                                            />
                                        </VStack>
                                    </TabPanel>
                                </TabPanels>
                            </Tabs>
                        </Box>
                    ))}
                </AwaitAPI>
            ))}
        </AwaitAPI>
    );
}
