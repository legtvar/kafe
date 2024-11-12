import {
    Box,
    Button,
    Heading,
    HStack,
    ListItem,
    Tab,
    TabList,
    TabPanel,
    TabPanels,
    Tabs,
    Text,
    UnorderedList,
    useColorModeValue,
} from '@chakra-ui/react';
import { t } from 'i18next';
import moment from 'moment';
import { useCallback } from 'react';
import { IoSaveOutline, IoWarning } from 'react-icons/io5';
import { useNavigate } from 'react-router-dom';
import { API } from '../../../api/API';
import { useAuth } from '../../../hooks/Caffeine';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { AllPermissions, SystemHRIB } from '../../../schemas/generic';
import { useTitle } from '../../../utils/useTitle';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { RightsEditor } from '../../utils/RightsEditor';
import { SendAPI } from '../../utils/SendAPI';

interface ISystemComponentProps {}

export function SystemComponent(props: ISystemComponentProps) {
    const borderColor = useColorModeValue('gray.300', 'gray.700');
    const { border } = useColorScheme();
    const { user } = useAuth();
    const navigate = useNavigate();
    useTitle(t('title.system'));

    const readonly = !user?.permissions['system']?.includes('write');

    const getStatus = useCallback((api: API) => api.system.status(), []);

    return (
        <OutletOrChildren>
            <AwaitAPI request={useCallback((api) => api.entities.perms.getById(SystemHRIB), [])}>
                {(perms) => (
                    <Box m={6} pb={12}>
                        <SendAPI
                            request={(api, value) => api.entities.perms.update(value)}
                            onSubmited={() => navigate(0) /* Refresh the page */}
                            value={perms}
                            repeatable={true}
                        >
                            {(onSubmit, status) => (
                                <>
                                    <HStack>
                                        <Heading
                                            fontSize="4xl"
                                            fontWeight="semibold"
                                            as="h2"
                                            lineHeight="tight"
                                            mr="auto"
                                            mb={2}
                                        >
                                            {t('system.title').toString()}
                                        </Heading>
                                        {!readonly && (
                                            <Button
                                                leftIcon={<IoSaveOutline />}
                                                colorScheme="blue"
                                                ml={{
                                                    base: '0',
                                                    xl: '4',
                                                }}
                                                mt={{
                                                    base: '2',
                                                    xl: '0',
                                                }}
                                                onClick={onSubmit}
                                                isLoading={status === 'sending'}
                                                isDisabled={status === 'sending'}
                                            >
                                                {t('generic.save').toString()}
                                            </Button>
                                        )}
                                    </HStack>

                                    <HStack color="red.500" mb={8}>
                                        <IoWarning />
                                        <Text>{t('system.warning').toString()}</Text>
                                    </HStack>
                                </>
                            )}
                        </SendAPI>
                        <Tabs>
                            <TabList>
                                <Tab>{t('system.rights').toString()}</Tab>
                                <Tab>{t('system.status').toString()}</Tab>
                            </TabList>

                            <TabPanels pt={6}>
                                <TabPanel>
                                    <RightsEditor
                                        perms={perms}
                                        options={AllPermissions}
                                        explanation={{
                                            read: t('rights.groups.system.read').toString(),
                                            write: t('rights.groups.system.write').toString(),
                                            inspect: t('rights.groups.system.inspect').toString(),
                                            append: t('rights.groups.system.append').toString(),
                                            review: t('rights.groups.system.review').toString(),
                                        }}
                                        readonly={readonly}
                                    />
                                </TabPanel>
                                <TabPanel>
                                    <AwaitAPI request={getStatus}>
                                        {(status) => (
                                            <>
                                                <Heading as="h1" fontSize="2xl">
                                                    {status.name}
                                                </Heading>
                                                <UnorderedList>
                                                    <ListItem>
                                                        Version <strong>{status.version}</strong>
                                                    </ListItem>
                                                    <ListItem>
                                                        Commit <strong>{status.commit}</strong>{' '}
                                                        {moment(status.commitDate).calendar()}
                                                    </ListItem>
                                                    <ListItem>
                                                        {t('system.runningFrom')}{' '}
                                                        <strong>
                                                            {moment(status.runningSince).format(
                                                                'YYYY-MM-DD HH:mm:ss Z',
                                                            )}
                                                        </strong>
                                                    </ListItem>
                                                </UnorderedList>
                                            </>
                                        )}
                                    </AwaitAPI>
                                </TabPanel>
                            </TabPanels>
                        </Tabs>
                    </Box>
                )}
            </AwaitAPI>
        </OutletOrChildren>
    );
}
