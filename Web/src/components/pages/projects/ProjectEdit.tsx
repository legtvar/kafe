import {
    Box,
    Button,
    Flex,
    HStack,
    Heading,
    Stack,
    Tab,
    TabList,
    TabPanel,
    TabPanels,
    Tabs,
    Text,
    VStack,
} from '@chakra-ui/react';
import { t } from 'i18next';
import { useCallback } from 'react';
import { AiOutlineUnlock } from 'react-icons/ai';
import { BsFillExclamationTriangleFill, BsX } from 'react-icons/bs';
import { IoSaveOutline } from 'react-icons/io5';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { API } from '../../../api/API';
import { EntityPermissions } from '../../../data/EntityPermissions';
import { Project } from '../../../data/Project';
import { useAuth } from '../../../hooks/Caffeine';
import { observeAbstactType } from '../../utils/AbstractTypeObserver';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { RightsEditor } from '../../utils/RightsEditor';
import { SendAPI } from '../../utils/SendAPI';
import { Status } from '../../utils/Status';
import { ArtifactGroupUpload } from '../../utils/Upload/ArtifactGroup';
import { WithTitle } from '../../utils/WithTitle';
import { AddReview } from './AddReview';
import { ProjectStatus } from './ProjectStatus';
import { ProjectTags } from './ProjectTags';
import { ReviewList } from './ReviewList';
import { ProjectBasicInfo } from './create/ProjectBasicInfo';

interface IProjectEditProps {}

export function ProjectEdit(props: IProjectEditProps) {
    const { id } = useParams();
    const { user } = useAuth();
    const navigate = useNavigate();

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    const getPerms = useCallback((api: API) => api.entities.perms.getById(id), [id]);

    return (
        <AwaitAPI
            request={useCallback((api) => api.projects.getById(id), [id])}
            error={(resp) => {
                return <Status statusCode={resp.response.status} log={resp.response.detail} embeded />;
            }}
        >
            {observeAbstactType((project: Project) => (
                <AwaitAPI request={getPerms}>
                    {observeAbstactType((perms: EntityPermissions) => (
                        <Box m={6} pb={12}>
                            <WithTitle title={t('title.project', { project: project.getName() })} />
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
                                    {project.getName()}
                                </Heading>
                                <SendAPI
                                    value={project}
                                    request={(api: API, value: Project) => api.projects.update(value)}
                                    onSubmited={() => navigate(0)}
                                    repeatable={true}
                                >
                                    {(saveProject, projectSaveStatus) => (
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
                                                    isDisabled={project.changed.size === 0 && perms.changed.size === 0}
                                                    onClick={() => {
                                                        if (project.changed.size > 0) {
                                                            saveProject();
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
                                    <Button leftIcon={<BsX />}>{t('projectEdit.quit').toString()}</Button>
                                </Link>
                            </Flex>
                            <ProjectTags project={project} />
                            <Tabs>
                                <TabList>
                                    <Tab>{t('projectEdit.tabs.status').toString()}</Tab>
                                    <Tab>{t('projectEdit.tabs.info').toString()}</Tab>
                                    <Tab>{t('projectEdit.tabs.files').toString()}</Tab>
                                    <Tab>{t('projectEdit.tabs.rights').toString()}</Tab>
                                    {project.userPermissions.includes('review') && (
                                        <Tab>
                                            <AiOutlineUnlock />
                                            <Text pl={2}>{t('projectEdit.tabs.admin').toString()}</Text>
                                        </Tab>
                                    )}
                                </TabList>

                                <TabPanels pt={6}>
                                    <TabPanel mt={-4}>
                                        <ProjectStatus projectId={id} />

                                        <Heading as="h2" fontSize="lg" mb={4} pt={24}>
                                            {t('project.admin.archive').toString()}
                                        </Heading>
                                        <ReviewList project={project} />
                                    </TabPanel>
                                    <TabPanel>
                                        <ProjectBasicInfo project={project} noSelfSubmit />
                                    </TabPanel>
                                    <TabPanel>
                                        <HStack mb={6} color="yellow.500">
                                            <BsFillExclamationTriangleFill />
                                            <Text fontStyle="italic">{t('general.autosave').toString()}</Text>
                                        </HStack>
                                        <Stack spacing={8} direction="column">
                                            {Object.entries(project.blueprint.artifactBlueprints).map(
                                                ([slotName, blueprint], i) => (
                                                    <ArtifactGroupUpload
                                                        key={i}
                                                        project={project}
                                                        artifactBlueprint={blueprint!}
                                                        slotName={slotName}
                                                    />
                                                ),
                                            )}
                                        </Stack>
                                    </TabPanel>
                                    <TabPanel>
                                        <VStack align="stretch">
                                            <RightsEditor
                                                perms={perms}
                                                options={['read', 'write', 'append']}
                                                explanation={{
                                                    read: t('rights.groups.project.read').toString(),
                                                    write: t('rights.groups.project.write').toString(),
                                                }}
                                            />
                                        </VStack>
                                    </TabPanel>
                                    <TabPanel>
                                        <Heading as="h2" fontSize="lg" mb={4}>
                                            {t('project.admin.new').toString()}
                                        </Heading>
                                        <AddReview project={project} />
                                        <Heading as="h2" fontSize="lg" mb={4} mt={8}>
                                            {t('project.admin.archive').toString()}
                                        </Heading>
                                        <ReviewList project={project} />
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
