import { Box, Button, Flex, Heading, Stack, Tab, TabList, TabPanel, TabPanels, Tabs, Text } from '@chakra-ui/react';
import { t } from 'i18next';
import { AiOutlineUnlock } from 'react-icons/ai';
import { BsX } from 'react-icons/bs';
import { Link, useParams } from 'react-router-dom';
import { Project } from '../../../data/Project';
import { useAuth } from '../../../hooks/Caffeine';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { RightsEditor } from '../../utils/RightsEditor';
import { Status } from '../../utils/Status';
import { ArtifactGroupUpload } from '../../utils/Upload/ArtifactGroup';
import { AddReview } from './AddReview';
import { ProjectBasicInfo } from './create/ProjectBasicInfo';
import { ProjectStatus } from './ProjectStatus';
import { ProjectTags } from './ProjectTags';
import { ReviewList } from './ReviewList';

interface IProjectEditProps {}

export function ProjectEdit(props: IProjectEditProps) {
    const { id } = useParams();
    const { user } = useAuth();

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    const tagsForAdmin = ['ProjectReview', 'Administration'];

    return (
        <AwaitAPI request={(api) => api.projects.getById(id)} error={<Status statusCode={404} embeded />}>
            {(project: Project) => (
                <Box m={6} pb={12}>
                    <Flex mb={2}>
                        <Heading fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" mr="auto">
                            {project.getName()}
                        </Heading>
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
                            {/* <Tab>{t('projectEdit.tabs.rights').toString()}</Tab> */}
                            {user &&
                                user.capabilities.some((cap) => tagsForAdmin.some((tag) => cap.startsWith(tag))) && (
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
                                <ProjectBasicInfo project={project} />
                            </TabPanel>
                            <TabPanel>
                                <Stack spacing={8} direction="column">
                                    {Object.entries(project.blueprint.artifactBlueprints).map(
                                        ([slotName, blueprint]) => (
                                            <ArtifactGroupUpload
                                                project={project}
                                                artifactBlueprint={blueprint!}
                                                slotName={slotName}
                                            />
                                        ),
                                    )}
                                </Stack>
                            </TabPanel>
                            {/* <TabPanel>
                                <RightsEditor
                                    item={project}
                                    explanation={{
                                        read: t('rights.groups.project.read').toString(),
                                        write: t('rights.groups.project.write').toString(),
                                        inspect: t('rights.groups.project.inspect').toString(),
                                        append: t('rights.groups.project.append').toString(),
                                    }}
                                />
                            </TabPanel> */}
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
            )}
        </AwaitAPI>
    );
}
