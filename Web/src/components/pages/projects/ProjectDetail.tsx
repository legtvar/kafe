import { Box, Button, Flex, Heading, SimpleGrid } from '@chakra-ui/react';
import { t } from 'i18next';
import { AiOutlineEdit } from 'react-icons/ai';
import { Link, useParams } from 'react-router-dom';
import { Project } from '../../../data/Project';
import { ContentThumbnail } from '../../media/ContentThumbnail';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { ProjectAuthorList } from '../../utils/ProjectAuthorList';
import { Status } from '../../utils/Status';
import { WithTitle } from '../../utils/WithTitle';
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
                        <WithTitle title={t('title.project', { project: project.getName() })} />
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
                                    {t('project.crew')}
                                </Heading>
                                <ProjectAuthorList authors={project.crew} />
                            </>
                        )}
                        {project.cast.length > 0 && (
                            <>
                                <Heading as="h3" size="md" pt={12} pb={4}>
                                    {t('project.cast')}
                                </Heading>
                                <ProjectAuthorList authors={project.cast} />
                            </>
                        )}

                        {project.artifacts.length > 0 && (
                            <>
                                <Heading as="h3" size="md" pt={12} pb={4}>
                                    {t('project.media')}
                                </Heading>
                                <SimpleGrid
                                    spacing={8}
                                    columns={{
                                        base: 1,
                                        sm: 1,
                                        md: 2,
                                        lg: 3,
                                        xl: 4,
                                    }}
                                >
                                    {project.artifacts.map((artifact, i) => (
                                        <ContentThumbnail artifact={artifact} key={i} />
                                    ))}
                                </SimpleGrid>
                            </>
                        )}
                    </Box>
                )}
            </AwaitAPI>
        </OutletOrChildren>
    );
}
