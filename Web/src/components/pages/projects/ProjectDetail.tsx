import { Box, Button, Flex, Heading, SimpleGrid } from '@chakra-ui/react';
import { t } from 'i18next';
import { useCallback } from 'react';
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
import { User } from '../../../data/User';

interface IProjectDetailProps {}

export function ProjectDetail(props: IProjectDetailProps) {
    const { id } = useParams();

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    return (
        <OutletOrChildren>
            <AwaitAPI
                request={useCallback((api) => api.projects.getById(id), [id])}
                error={(resp) => {
                    return <Status statusCode={resp.response.status} log={resp.response.detail} embeded />;
                }}
            >
                {(project: Project) => {
                    return (
                        <Box m={6} pb={12}>
                            <WithTitle title={t('title.project', { project: project.getName() })} />
                            <Flex
                                direction={{
                                    base: 'column',
                                    md: 'row',
                                }}
                                gap={4}
                                mb={2}
                                alignItems="start"
                            >
                                <Heading fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" mr="auto">
                                    {project.getName()}
                                </Heading>
                                <Link to="edit">
                                    <Button leftIcon={<AiOutlineEdit />}>{t('generic.edit').toString()}</Button>
                                </Link>
                            </Flex>
                            
                            {project.ownerId && (
                                <AwaitAPI
                                    request={(api) => api.accounts.info.getById(project.ownerId!)}
                                    error={() => <></>}
                                >
                                    {(owner: User) => (
                                        <Box color="gray.500" fontStyle="italic" mb={4}>
                                            {owner.name && owner.name} {owner.uco && (<span>({owner.uco})</span>)}
                                        </Box>
                                    )}
                                </AwaitAPI>
                            )}
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
                    );
                }}
            </AwaitAPI>
        </OutletOrChildren>
    );
}
