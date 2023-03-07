import { Box, Button, Flex, Heading, Stack, Tab, TabList, TabPanel, TabPanels, Tabs, Text } from '@chakra-ui/react';
import { t } from 'i18next';
import { AiOutlineUnlock } from 'react-icons/ai';
import { BsX } from 'react-icons/bs';
import { Link, useParams } from 'react-router-dom';
import { Project } from '../../../data/Project';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { Status } from '../../utils/Status';
import { UploadArtifact } from '../../utils/Upload/UploadArtifact';
import { ProjectBasicInfo } from './create/ProjectBasicInfo';
import { ProjectTags } from './ProjectTags';
import { StatusCheck } from './StatusCheck';

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
                            <Tab>Stav</Tab>
                            <Tab>Základní informace</Tab>
                            <Tab>Soubory</Tab>
                            <Tab>
                                <AiOutlineUnlock />
                                <Text pl={2}>Přístup administrátora</Text>
                            </Tab>
                        </TabList>

                        <TabPanels pt={6}>
                            <TabPanel mt={-4}>
                                <StatusCheck status="ok">Základní informace jsou dostatečně vyplněny</StatusCheck>
                                <StatusCheck status="ok">Soubory jsou nahrány</StatusCheck>
                                <StatusCheck
                                    status="nok"
                                    details={
                                        <>
                                            Do ullamco irure ut dolore. Cillum dolor consectetur sint nulla ut ea labore
                                            cupidatat. Aliqua consectetur minim nulla exercitation do fugiat nisi ex
                                            sunt ea elit anim esse ut. Veniam eiusmod est pariatur cupidatat labore. Ut
                                            ullamco nulla ipsum irure qui laboris minim exercitation tempor dolore
                                            consequat incididunt. Enim commodo non adipisicing ex.
                                        </>
                                    }
                                >
                                    Soubory prošly automatizovanou kontrolou
                                </StatusCheck>
                                <StatusCheck status="unknown">Projekt prošel kontrolou technikem</StatusCheck>
                                <StatusCheck status="unknown">Projekt byl přijat</StatusCheck>
                            </TabPanel>
                            <TabPanel>
                                <ProjectBasicInfo project={project} />
                            </TabPanel>
                            <TabPanel>
                                <Stack spacing={8} direction="column">
                                    <UploadArtifact
                                        projectId={'0INB9KB5bhz'}
                                        artifactFootprint={{
                                            id: 'test',
                                            name: {
                                                iv: 'Film',
                                            },
                                            shards: [
                                                {
                                                    id: 'aaaaa',
                                                    kind: 'Video',
                                                },
                                                {
                                                    id: 'bbbbb',
                                                    kind: 'Subtitles',
                                                },
                                            ],
                                        }}
                                    />
                                </Stack>
                            </TabPanel>
                            <TabPanel>Tady bude časem možnost poslat feedback a schválit projekt</TabPanel>
                        </TabPanels>
                    </Tabs>
                </Box>
            )}
        </AwaitAPI>
    );
}
