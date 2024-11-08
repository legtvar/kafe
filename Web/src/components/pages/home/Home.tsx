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
import ChakraUIRenderer from 'chakra-ui-markdown-renderer';
import { t } from 'i18next';
import Countdown from 'react-countdown';
import { IoCubeOutline } from 'react-icons/io5';
import Markdown from 'react-markdown';
import { Link } from 'react-router-dom';
import { useAuthLink } from '../../../hooks/useAuthLink';
import { useTitle } from '../../../utils/useTitle';
import { Brand } from '../../brand/Brand';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { OutletOrChildren } from '../../utils/OutletOrChildren';

interface IHomeProps {}

export function Home(props: IHomeProps) {
    useTitle(t('home.title'));
    const CountdownItem = (props: { children: number; title: string }) => (
        <Box textAlign="center" py={8} px={4} wordBreak="keep-all">
            <Text fontSize="2em" fontWeight="bold">
                {props.children}
            </Text>
            <Text>{props.title}</Text>
        </Box>
    );

    const DESCRIPTION_LENGTH = 200;

    return (
        <OutletOrChildren>
            <VStack w="full" px={6} py={4} alignItems="stretch">
                <HStack spacing={6} alignItems="center" mb={6}>
                    <Center fontSize="6xl" h="auto">
                        <Brand />
                    </Center>
                    <VStack alignItems="start">
                        <Heading size="2xl" mt={0}>
                            {t('home.title')}
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
                <AwaitAPI request={(api) => api.groups.getAll()}>
                    {(groups) => (
                        <Box w="full" overflowX="auto" py={4} scrollSnapType="x mandatory">
                            <HStack alignItems="start">
                                {groups
                                    .filter((group) => group.isOpen)
                                    .map((group, i) => (
                                        <Card w="sm" maxW="100%" key={i} flexShrink={0} scrollSnapAlign="start">
                                            <CardBody>
                                                <Stack spacing="3">
                                                    <Heading size="md">{group.getName()}</Heading>
                                                    {group.description && (
                                                        <Box mb={4}>
                                                            <Markdown components={ChakraUIRenderer()} skipHtml>
                                                                {group
                                                                    .getDescription()
                                                                    .substring(0, DESCRIPTION_LENGTH) +
                                                                    (group.getDescription().length > DESCRIPTION_LENGTH
                                                                        ? '...'
                                                                        : '')}
                                                            </Markdown>
                                                        </Box>
                                                    )}
                                                    {group.deadline && (
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
                                                                            <CountdownItem title={t('countdown.days')}>
                                                                                {days}
                                                                            </CountdownItem>
                                                                            <CountdownItem title={t('countdown.hours')}>
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

                {/* <Heading size="lg" mb={4} mt={6}>
                    <HStack>
                        <IoFolderOpenOutline />
                        <Text>{t('home.myProjects')}</Text>
                    </HStack>
                </Heading>
                <AwaitAPI request={(api) => api.projects.getAll()}>
                    {(projects) => {
                        return (
                            <Box w="full" overflowX="auto" py={4} scrollSnapType="x mandatory">
                                <HStack alignItems="start">
                                    {projects
                                        .filter((project) => project.userPermissions.includes('write'))
                                        .map((project, i) => (
                                            <Card w="sm" maxW="100%" key={i} flexShrink={0} scrollSnapAlign="start">
                                                <CardBody>
                                                    <Stack spacing="3">
                                                        <Heading size="md">{project.getName()}</Heading>
                                                        {project.description && (
                                                            <Box mb={4}>
                                                                {project
                                                                    .getDescription()
                                                                    .substring(0, DESCRIPTION_LENGTH)}
                                                                {project.getDescription().length > DESCRIPTION_LENGTH &&
                                                                    '...'}
                                                            </Box>
                                                        )}
                                                    </Stack>
                                                </CardBody>
                                                <Divider />
                                                <CardFooter>
                                                    <ButtonGroup spacing="2">
                                                        <Link to={useAuthLink(`/groups/${project.id}`)}>
                                                            <Button variant="solid" colorScheme="brand">
                                                                {t('project.details').toString()}
                                                            </Button>
                                                        </Link>
                                                        <Link to={useAuthLink(`/groups/${project.id}/edit`)}>
                                                            <Button variant="outline" colorScheme="brand">
                                                                {t('project.edit').toString()}
                                                            </Button>
                                                        </Link>
                                                    </ButtonGroup>
                                                </CardFooter>
                                            </Card>
                                        ))}
                                </HStack>
                            </Box>
                        );
                    }}
                </AwaitAPI> */}
            </VStack>
        </OutletOrChildren>
    );
}
