import {
    Box,
    Button,
    ButtonGroup,
    Card,
    CardBody,
    CardFooter,
    Divider,
    Heading,
    HStack,
    SimpleGrid,
    Stack,
    Text,
    VStack,
} from '@chakra-ui/react';
import { t } from 'i18next';
import Countdown from 'react-countdown';
import { Link } from 'react-router-dom';
import { useTitle } from '../../../utils/useTitle';
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

    return (
        <OutletOrChildren>
            <VStack w="full" px={6} py={4} alignItems="stretch">
                <AwaitAPI request={(api) => api.groups.getAll()}>
                    {(groups) => (
                        <>
                            <Heading size="lg" mb={4}>
                                {t('home.openGroups')}
                            </Heading>
                            <Box w="full" overflowX="auto" py={4}>
                                <HStack>
                                    {groups
                                        .filter((group) => group.isOpen)
                                        .map((group, i) => (
                                            <Card w="sm" key={i}>
                                                <CardBody>
                                                    <Stack spacing="3">
                                                        <Heading size="md">{group.getName()}</Heading>
                                                        {group.description && (
                                                            <Box mb={4}>{group.getDescription()}</Box>
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
                                                        <Link to={`/auth/groups/${group.id}/create`}>
                                                            <Button variant="solid" colorScheme="brand">
                                                                {t('createProject.signUp').toString()}
                                                            </Button>
                                                        </Link>
                                                        <Link to={`/auth/groups/${group.id}/create`}>
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
                        </>
                    )}
                </AwaitAPI>

                <AwaitAPI request={(api) => api.projects.getAll()}>
                    {(projects) => (
                        <>
                            <Heading size="lg" mb={4} mt={10}>
                                {t('home.myProjects')}
                            </Heading>
                            <Box w="full" overflowX="auto" py={4}>
                                <HStack>
                                    {projects
                                        .filter(
                                            (project) =>
                                                project.userPermissions.includes(
                                                    'write',
                                                ) /* TODO: Apply correct permissions */,
                                        )
                                        .map((project, i) => (
                                            <Card w="sm" key={i}>
                                                <CardBody>
                                                    <Stack spacing="3">
                                                        <Heading size="md">{project.getName()}</Heading>
                                                        {project.description && (
                                                            <Box mb={4}>{project.getDescription()}</Box>
                                                        )}
                                                    </Stack>
                                                </CardBody>
                                                <Divider />
                                                <CardFooter>
                                                    <ButtonGroup spacing="2">
                                                        <Link to={`/auth/groups/${project.id}/edit`}>
                                                            <Button variant="solid" colorScheme="brand">
                                                                {t('project.edit').toString()}
                                                            </Button>
                                                        </Link>
                                                        <Link to={`/auth/groups/${project.id}`}>
                                                            <Button variant="outline" colorScheme="brand">
                                                                {t('project.details').toString()}
                                                            </Button>
                                                        </Link>
                                                    </ButtonGroup>
                                                </CardFooter>
                                            </Card>
                                        ))}
                                </HStack>
                            </Box>
                        </>
                    )}
                </AwaitAPI>
            </VStack>
        </OutletOrChildren>
    );
}
