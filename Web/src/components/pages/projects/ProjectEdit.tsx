import { Box, Button, Flex, Heading, Stack, Tab, TabList, TabPanel, TabPanels, Tabs } from '@chakra-ui/react';
import { t } from 'i18next';
import { BsX } from 'react-icons/bs';
import { Link, useParams } from 'react-router-dom';
import { Project } from '../../../data/Project';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { Status } from '../../utils/Status';
import { UploadArtifact } from '../../utils/Upload/UploadArtifact';
import { ProjectBasicInfo } from './create/ProjectBasicInfo';
import { ProjectStatus } from './ProjectStatus';
import { ProjectTags } from './ProjectTags';

interface IProjectEditProps {}

export function ProjectEdit(props: IProjectEditProps) {
    const { id } = useParams();

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    return (
        <AwaitAPI request={(api) => api.projects.getById(id)} error={<Status statusCode={404} embeded />}>
            {(project: Project) => (
                <Box m={6} pb={12}>
                    <Flex mb={2}>
                        <Heading fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" mr="auto">
                            {project.getName()}
                        </Heading>
                        <Link to="..">
                            <Button leftIcon={<BsX />} mr={4}>
                                {t('generic.cancel').toString()}
                            </Button>
                        </Link>
                    </Flex>
                    <ProjectTags project={project} />
                    <Tabs>
                        <TabList>
                            <Tab>{t('projectEdit.tabs.status').toString()}</Tab>
                            <Tab>{t('projectEdit.tabs.info').toString()}</Tab>
                            <Tab>{t('projectEdit.tabs.files').toString()}</Tab>
                            {/* <Tab>
                                <AiOutlineUnlock />
                                <Text pl={2}>{t('projectEdit.tabs.admin').toString()}</Text>
                            </Tab> */}
                        </TabList>

                        <TabPanels pt={6}>
                            <TabPanel mt={-4}>
                                <ProjectStatus projectId={id} />
                            </TabPanel>
                            <TabPanel>
                                <ProjectBasicInfo project={project} />
                            </TabPanel>
                            <TabPanel>
                                <Stack spacing={8} direction="column">
                                    {project.blueprint.artifactBlueprints.map((blueprint) => (
                                        <UploadArtifact projectId={id} artifactBlueprint={blueprint} />
                                    ))}
                                </Stack>
                            </TabPanel>
                            <TabPanel></TabPanel>
                        </TabPanels>
                    </Tabs>
                </Box>
            )}
        </AwaitAPI>
    );
}
