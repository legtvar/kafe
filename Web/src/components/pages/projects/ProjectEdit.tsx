import { Box, Heading, HStack, Tag, Text } from '@chakra-ui/react';
import { Link, useParams } from 'react-router-dom';
import { Project } from '../../../data/Project';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { getPrefered } from '../../../utils/preferedLanguage';
import { ContentViewer } from '../../media/ContentViewer';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { ProjectAuthorList } from '../../utils/ProjectAuthorList';
import { Status } from '../../utils/Status';
import { VisibilityTag } from '../../utils/VisibilityTag';

interface IProjectEditProps {}

export function ProjectEdit(props: IProjectEditProps) {
    const { id } = useParams();
    const { bgDarker } = useColorScheme();

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    return (
        <AwaitAPI request={(api) => api.projects.getById(id)} error={<Status statusCode={404} embeded />}>
            {(project: Project) => (
                <Box m={6} pb={12}>
                    <Box fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" mb={2} isTruncated>
                        {project.getName()}
                    </Box>
                    <HStack spacing={2} mb={6}>
                        {project.getGenere().length > 0 && <Text pr={8}>{project.getGenere()}</Text>}
                        <Link to={`/auth/groups/${project.projectGroupId}`}>
                            <Tag bg={bgDarker}>{project.getGroupName()}</Tag>
                        </Link>
                        <VisibilityTag visibility={project.visibility} />
                    </HStack>
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
    );
}
