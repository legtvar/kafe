import {
    Box,
    Button,
    Heading,
    HStack,
    Tab,
    TabList,
    TabPanel,
    TabPanels,
    Tabs,
    Text,
    useColorModeValue,
} from '@chakra-ui/react';
import { t } from 'i18next';
import { IoWarning } from 'react-icons/io5';
import { useAuth } from '../../../hooks/Caffeine';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { RightsEditor } from '../../utils/RightsEditor';
import { Rights } from '../../utils/RightsItem';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { SystemHRIB } from '../../../schemas/generic';
import { SendAPI } from '../../utils/SendAPI';

interface ISystemComponentProps {}

export function SystemComponent(props: ISystemComponentProps) {
    const borderColor = useColorModeValue('gray.300', 'gray.700');
    const { border } = useColorScheme();
    const { user } = useAuth();

    return (
        <OutletOrChildren>
            <AwaitAPI request={(api) => api.entities.perms.getById(SystemHRIB)}>
                {(perms) => (
                    <Box m={6} pb={12}>
                        <SendAPI
                            request={(api, value) => api.entities.perms.update(value)}
                            onSubmited={() => {}}
                            value={perms}
                            repeatable={true}
                        >
                            {(onSubmit, status, error) => (
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
                                        <Button
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
                                            {t('generic.save').toString()} ({status})
                                        </Button>
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
                                {/* <Tab>{t('system.homepage').toString()}</Tab> */}
                            </TabList>

                            <TabPanels pt={6}>
                                <TabPanel>
                                    <RightsEditor
                                        perms={perms}
                                        options={[
                                            Rights.READ,
                                            Rights.WRITE,
                                            Rights.APPEND,
                                            Rights.INSPECT,
                                            Rights.REVIEW,
                                        ]}
                                        explanation={{
                                            read: t('rights.groups.system.read').toString(),
                                            write: t('rights.groups.system.write').toString(),
                                            inspect: t('rights.groups.system.inspect').toString(),
                                            append: t('rights.groups.system.append').toString(),
                                            review: t('rights.groups.system.review').toString(),
                                        }}
                                    />
                                </TabPanel>
                                {/* <TabPanel>{t('generic.construction').toString()}</TabPanel> */}
                            </TabPanels>
                        </Tabs>
                    </Box>
                )}
            </AwaitAPI>
        </OutletOrChildren>
    );
}
