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
    useTitle(t("homeFestival.title"));
    const CountdownItem = (props: { children: number; title: string }) => (
        <Box textAlign="center" py={8} px={4}>
            <Text fontSize="2em" fontWeight="bold">
                {props.children}
            </Text>
            <Text>{props.title}</Text>
        </Box>
    );

    return (
        <OutletOrChildren>
            <AwaitAPI request={(api) => api.groups.getAll()}>
                {(data: Group[]) => (
                    <Box p={8}>
                        {(() => {
                            const filtered = data.filter((data) => data.isOpen);

                            if (filtered.length > 0)
                                return (
                                    <>
                                        <Box fontSize="xl" as="h2" lineHeight="tight" color="gray.500" isTruncated>
                                            {t('homeFestival.title').toString()}
                                        </Box>
                                        {filtered.map((group) => (
                                            <>
                                                <Box
                                                    fontSize="4xl"
                                                    fontWeight="semibold"
                                                    as="h2"
                                                    lineHeight="tight"
                                                    mb={8}
                                                    isTruncated
                                                >
                                                    {group.getName()}
                                                </Box>

                                                <IntroText groupName={group.getName()} />

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
                                        ))}
                                    </>
                                );
                        })()}
                    </Box>
                )}
            </AwaitAPI>
        </OutletOrChildren>
    );
}
