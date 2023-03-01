import { Box, Heading, Stack, Tag } from '@chakra-ui/react';
import { Link, useParams } from 'react-router-dom';
import { Project } from '../../../data/Project';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { ProjectAuthorList } from '../../utils/ProjectAuthorList';
import { Status } from '../../utils/Status';

interface IProjectDetailProps {}

export function ProjectDetail(props: IProjectDetailProps) {
    const { id } = useParams();
    const { bgDarker } = useColorScheme();

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    return (
        <AwaitAPI request={(api) => api.projects.getById(id)} error={<Status statusCode={404} embeded />}>
            {(project: Project) => (
                <Stack spacing={4} m={6} direction="column">
                    <Box fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" isTruncated>
                        {project.getName()}
                    </Box>
                    <Box>
                        <Link to={`/auth/groups/${project.projectGroupId}`}>
                            <Tag bg={bgDarker}>{project.getGroupName()}</Tag>
                        </Link>
                    </Box>
                    <Box>{project.getDescription()}</Box>
                    {project.crew.length > 0 && (
                        <>
                            <Heading as="h3" size="md" pt={12}>
                                Štáb
                            </Heading>
                            <ProjectAuthorList authors={project.crew} />
                        </>
                    )}
                    {project.cast.length > 0 && (
                        <>
                            <Heading as="h3" size="md" pt={12}>
                                Herci
                            </Heading>
                            <ProjectAuthorList authors={project.cast} />
                        </>
                    )}
                </Stack>
            )}
        </AwaitAPI>
    );
}
