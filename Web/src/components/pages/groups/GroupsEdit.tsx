import { Box, Button, HStack, Heading, Tab, TabList, TabPanel, TabPanels, Tabs, VStack } from '@chakra-ui/react';
import { t } from 'i18next';
import { useCallback } from 'react';
import { BsX } from 'react-icons/bs';
import { IoSaveOutline } from 'react-icons/io5';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { API } from '../../../api/API';
import { EntityPermissions } from '../../../data/EntityPermissions';
import { Group } from '../../../data/Group';
import { observeAbstactType } from '../../utils/AbstractTypeObserver';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { RightsEditor } from '../../utils/RightsEditor';
import { SendAPI } from '../../utils/SendAPI';
import { Status } from '../../utils/Status';
import { WithTitle } from '../../utils/WithTitle';
import { GroupBasicInfo } from './GroupBasicInfo';

interface IGroupsEditProps {}

export function GroupsEdit(props: IGroupsEditProps) {
    const { id } = useParams();
    const navigate = useNavigate();

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    const getPerms = useCallback((api: API) => api.entities.perms.getById(id), [id]);

    return (
        <AwaitAPI
            request={useCallback((api) => api.groups.getById(id), [id])}
            error={(resp) => {
                return <Status statusCode={resp.response.status} log={resp.response.detail} embeded />;
            }}
        >
            {observeAbstactType((group: Group) => (
                <AwaitAPI request={getPerms}>
                    {observeAbstactType((perms: EntityPermissions) => (
                        <Box m={6} pb={12}>
                            <WithTitle title={t('title.group', { group: group.getName() })} />
                            <HStack mb={2}>
                                <Heading fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" mr="auto">
                                    {group.getName()}
                                </Heading>
                                <SendAPI
                                    value={group}
                                    request={(api: API, value: Group) => api.groups.update(value)}
                                    onSubmited={() => navigate(0)}
                                    repeatable={true}
                                >
                                    {(saveGroup, groupSaveStatus) => (
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
                                                    isDisabled={group.changed.size === 0 && perms.changed.size === 0}
                                                    onClick={() => {
                                                        if (group.changed.size > 0) {
                                                            saveGroup();
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
                            </HStack>
                            <Tabs>
                                <TabList>
                                    <Tab>{t('groupsEdit.tabs.info').toString()}</Tab>
                                    <Tab>{t('groupsEdit.tabs.rights').toString()}</Tab>
                                </TabList>

                                <TabPanels pt={6}>
                                    <TabPanel>
                                        <GroupBasicInfo group={group} noSelfSubmit />
                                    </TabPanel>
                                    <TabPanel>
                                        <VStack align="stretch">
                                            <RightsEditor
                                                perms={perms}
                                                options={['read', 'write', 'inspect', 'append', 'review']}
                                                explanation={{
                                                    read: t('rights.groups.group.read').toString(),
                                                    write: t('rights.groups.group.write').toString(),
                                                    inspect: t('rights.groups.group.inspect').toString(),
                                                    append: t('rights.groups.group.append').toString(),
                                                    review: t('rights.groups.group.review').toString(),
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
