import {
    Box,
    Button,
    ButtonGroup,
    Card,
    CardBody,
    CardFooter,
    Center,
    Divider,
    Heading,
    HStack,
    SimpleGrid,
    Stack,
    Text,
    VStack,
} from '@chakra-ui/react';
import { t } from 'i18next';
import { useCallback } from 'react';
import Countdown from 'react-countdown';
import { IoCubeOutline } from 'react-icons/io5';
import { Link } from 'react-router-dom';
import { useOrganizations } from '../../../hooks/Caffeine';
import { useAuthLinkFunction } from '../../../hooks/useAuthLink';
import { useTitle } from '../../../utils/useTitle';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { ChakraMarkdown } from '../../utils/ChakraMarkdown';
import { OrganizationAvatar } from '../../utils/OrganizationAvatar/OrganizationAvatar';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { DateTime } from 'luxon';

interface IHomeProps {}

export function Home(props: IHomeProps) {
    useTitle(t('home.title'));
    const CountdownItem = (props: { children: number; title: string }) => (
        <Box textAlign="center" py={8} px={4} wordBreak="keep-all" whiteSpace="nowrap">
            <Text fontSize="2em" fontWeight="bold">
                {props.children}
            </Text>
            <Text>{props.title}</Text>
        </Box>
    );
    const useAuthLink = useAuthLinkFunction();

    const DESCRIPTION_LENGTH = 200;
    const { currentOrganization } = useOrganizations();

    return (
        <OutletOrChildren>
            <VStack w="full" px={6} py={4} alignItems="stretch">
                <HStack spacing={6} alignItems="center" mb={6}>
                    <Center fontSize="6xl" h="auto">
                        <OrganizationAvatar organization={currentOrganization!} size="xl" noHighlight />
                    </Center>
                    <VStack alignItems="start">
                        <Heading size="2xl" mt={0}>
                            {currentOrganization!.getName()}
                        </Heading>
                        <Text opacity={0.5}>{t('home.subtitle')}</Text>
                    </VStack>
                </HStack>
                <hr />
                <Heading size="lg" mb={4} mt={6}>
                    <HStack>
                        <IoCubeOutline />
                        <Text>{t('home.openGroups')}</Text>
                    </HStack>
                </Heading>
                <AwaitAPI
                    request={useCallback((api) => api.groups.getAll(currentOrganization?.id), [currentOrganization])}
                >
                    {(groups) => (
                        <Box w="full" overflowX="auto" py={4} scrollSnapType="x mandatory">
                            <HStack alignItems="start">
                                {groups.filter((group) => group.isOpen).length === 0 && (
                                    <Text w="full" textAlign="center" color="gray.500" fontStyle="italic">
                                        {t('home.noOpenGroups')}
                                    </Text>
                                )}
                                {groups
                                    .filter((group) => group.isOpen)
                                    .map((group, i) => (
                                        <Card w="sm" maxW="100%" key={i} flexShrink={0} scrollSnapAlign="start">
                                            <CardBody>
                                                <Stack spacing="3">
                                                    <Heading size="md">{group.getName()}</Heading>
                                                    {group.description && (
                                                        <Box mb={4}>
                                                            <ChakraMarkdown>
                                                                {group
                                                                    .getDescription()
                                                                    .substring(0, DESCRIPTION_LENGTH) +
                                                                    (group.getDescription().length > DESCRIPTION_LENGTH
                                                                        ? '...'
                                                                        : '')}
                                                            </ChakraMarkdown>
                                                        </Box>
                                                    )}
                                                    {group.deadline &&
                                                        DateTime.fromISO(group.deadline) > DateTime.now() && (
                                                            <Box mb={-6}>
                                                                <Countdown
                                                                    date={new Date(group.deadline)}
                                                                    renderer={({ days, hours, minutes, seconds }) => (
                                                                        <>
                                                                            <Box mb={-5}>
                                                                                {t('createProject.doNotforget')}
                                                                            </Box>
                                                                            <SimpleGrid
                                                                                columns={4}
                                                                                display="inline-grid"
                                                                                w="full"
                                                                            >
                                                                                <CountdownItem
                                                                                    title={t('countdown.days')}
                                                                                >
                                                                                    {days}
                                                                                </CountdownItem>
                                                                                <CountdownItem
                                                                                    title={t('countdown.hours')}
                                                                                >
                                                                                    {hours}
                                                                                </CountdownItem>
                                                                                <CountdownItem
                                                                                    title={t('countdown.minutes')}
                                                                                >
                                                                                    {minutes}
                                                                                </CountdownItem>
                                                                                <CountdownItem
                                                                                    title={t('countdown.seconds')}
                                                                                >
                                                                                    {seconds}
                                                                                </CountdownItem>
                                                                            </SimpleGrid>
                                                                        </>
                                                                    )}
                                                                />
                                                            </Box>
                                                        )}
                                                </Stack>
                                            </CardBody>
                                            <Divider />
                                            <CardFooter>
                                                <ButtonGroup spacing="2">
                                                    <Link to={useAuthLink(`/groups/${group.id}/create`)}>
                                                        <Button variant="solid" colorScheme="brand">
                                                            {t('createProject.signUp').toString()}
                                                        </Button>
                                                    </Link>
                                                    <Link to={useAuthLink(`/groups/${group.id}`)}>
                                                        <Button variant="outline" colorScheme="brand">
                                                            {t('createProject.details').toString()}
                                                        </Button>
                                                    </Link>
                                                </ButtonGroup>
                                            </CardFooter>
                                        </Card>
                                    ))}
                            </HStack>
                        </Box>
                    )}
                </AwaitAPI>
            </VStack>
        </OutletOrChildren>
    );
}
