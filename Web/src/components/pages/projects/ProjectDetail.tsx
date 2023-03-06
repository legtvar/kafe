import { Box, Button, Flex, Heading } from '@chakra-ui/react';
import { t } from 'i18next';
import { AiOutlineEdit } from 'react-icons/ai';
import { Link, useParams } from 'react-router-dom';
import { Project } from '../../../data/Project';
import { getPrefered } from '../../../utils/preferedLanguage';
import { ContentViewer } from '../../media/ContentViewer';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { ProjectAuthorList } from '../../utils/ProjectAuthorList';
import { Status } from '../../utils/Status';
import { ProjectTags } from './ProjectTags';

interface IProjectDetailProps {}

export function ProjectDetail(props: IProjectDetailProps) {
    const { id } = useParams();

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    return (
        <OutletOrChildren>
            <AwaitAPI request={(api) => api.projects.getById(id)} error={<Status statusCode={404} embeded />}>
                {(project: Project) => (
                    <Box m={6} pb={12}>
                        <Flex mb={2}>
                            <Heading fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" mr="auto">
                                {project.getName()}
                            </Heading>
                            <Link to="edit">
                                <Button leftIcon={<AiOutlineEdit />}>{t('generic.edit').toString()}</Button>
                            </Link>
                        </Flex>
                        <ProjectTags project={project} />
                        <Box>{project.getDescription()}</Box>

                        {project.crew.length > 0 && (
                            <>
                                <Heading as="h3" size="md" pt={12} pb={4}>
                                    Štáb
                                </Heading>
                                <ProjectAuthorList authors={project.crew} />
                            </>
                        )}
                        {project.cast.length > 0 && (
                            <>
                                <Heading as="h3" size="md" pt={12} pb={4}>
                                    Herci
                                </Heading>
                                <ProjectAuthorList authors={project.cast} />
                            </>
                        )}

                        {project.artifacts.map((artifact) => (
                            <Box key={artifact.id}>
                                <Heading as="h3" size="md" pt={12} pb={4}>
                                    {getPrefered(artifact.name)}
                                </Heading>
                                <Box w="100%" pb={4}>
                                    <ContentViewer artifact={artifact} />
                                </Box>
                            </Box>
                        ))}
                    </Box>
                )}
            </AwaitAPI>
        </OutletOrChildren>
    );
}
