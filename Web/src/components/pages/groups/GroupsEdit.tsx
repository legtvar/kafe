import { Box, Button, Flex, Heading, Tab, TabList, TabPanel, TabPanels, Tabs } from '@chakra-ui/react';
import { t } from 'i18next';
import { BsX } from 'react-icons/bs';
import { Link, useParams } from 'react-router-dom';
import { Group } from '../../../data/Group';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { RightsEditor } from '../../utils/RightsEditor';
import { Status } from '../../utils/Status';
import { GroupBasicInfo } from './GroupBasicInfo';

interface IGroupsEditProps {}

export function GroupsEdit(props: IGroupsEditProps) {
    const { id } = useParams();

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    return (
        <AwaitAPI request={(api) => api.groups.getById(id)} error={<Status statusCode={404} embeded />}>
            {(group: Group) => (
                <Box m={6} pb={12}>
                    <Flex mb={2}>
                        <Heading fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" mr="auto">
                            {group.getName()}
                        </Heading>
                        <Link to="..">
                            <Button leftIcon={<BsX />}>{t('generic.quitEdit').toString()}</Button>
                        </Link>
                    </Flex>
                    <Tabs>
                        <TabList>
                            <Tab>{t('groupsEdit.tabs.info').toString()}</Tab>
                            <Tab>{t('groupsEdit.tabs.rights').toString()}</Tab>
                        </TabList>

                        <TabPanels pt={6}>
                            <TabPanel>
                                <GroupBasicInfo group={group} />
                            </TabPanel>
                            <TabPanel>
                                <RightsEditor
                                    item={group}
                                    explanation={{
                                        read: t('rights.groups.group.read').toString(),
                                        write: t('rights.groups.group.write').toString(),
                                        inspect: t('rights.groups.group.inspect').toString(),
                                        append: t('rights.groups.group.append').toString(),
                                    }}
                                />
                            </TabPanel>
                        </TabPanels>
                    </Tabs>
                </Box>
            )}
        </AwaitAPI>
    );
}
