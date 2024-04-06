import { Box, Button, Flex, SimpleGrid, Spacer, Text } from '@chakra-ui/react';
import { t } from 'i18next';
import Countdown from 'react-countdown';
import { Link } from 'react-router-dom';
import { Group } from '../../../data/Group';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { IntroText } from '../projects/create/IntroText';
import { useTitle } from '../../../utils/useTitle';

interface IHomeFestivalProps {}

export function HomeFestival(props: IHomeFestivalProps) {
    useTitle(t('homeFestival.title'));
    const CountdownItem = (props: { children: number; title: string }) => (
        <Box textAlign="center" py={8} px={4} wordBreak="keep-all">
            <Text fontSize="2em" fontWeight="bold">
                {props.children}
            </Text>
            <Text>{props.title}</Text>
        </Box>
    );

    return (
        <OutletOrChildren>
            <Box p={8}>
                <Box
                    fontSize="4xl"
                    fontWeight="semibold"
                    as="h2"
                    lineHeight="tight"
                    mb={8}
                >
                    {t('homeFestival.title')}
                </Box>

                <IntroText />

                <AwaitAPI request={(api) => api.groups.getById('7rYVhQWyzk5')}>
                    {(group) => (
                        <>
                            {group.deadline && (
                                <Box>
                                    <Box fontWeight="bold" mt={12}>
                                        {t('createProject.doNotforget').toString()}
                                    </Box>
                                    <Countdown
                                        date={new Date(group.deadline)}
                                        renderer={({ days, hours, minutes, seconds }) => (
                                            <SimpleGrid columns={4} display="inline-grid">
                                                <CountdownItem title={t('countdown.days')}>
                                                    {days}
                                                </CountdownItem>
                                                <CountdownItem title={t('countdown.hours')}>
                                                    {hours}
                                                </CountdownItem>
                                                <CountdownItem title={t('countdown.minutes')}>
                                                    {minutes}
                                                </CountdownItem>
                                                <CountdownItem title={t('countdown.seconds')}>
                                                    {seconds}
                                                </CountdownItem>
                                            </SimpleGrid>
                                        )}
                                    />
                                </Box>
                            )}

                            <Flex direction="row" my={12}>
                                <Link to={`/auth/groups/${group.id}/create`}>
                                    <Button colorScheme="brand">
                                        {t('createProject.signUp').toString()}
                                    </Button>
                                </Link>
                                <Spacer />
                            </Flex>
                        </>
                    )}
                </AwaitAPI>
            </Box>
        </OutletOrChildren>
    );
}
