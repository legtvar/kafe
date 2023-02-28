import { Box, Stack, VStack } from '@chakra-ui/react';
import { useParams } from 'react-router-dom';
import { Project } from '../../../data/Project';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { Status } from '../../utils/Status';

interface IProjectDetailProps {}

export function ProjectDetail(props: IProjectDetailProps) {
    const { id } = useParams();

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
                    <Box>{project.getDescription()}</Box>
                    <VStack>
                        {project.authors.map((author) => (
                            <Box>{author}</Box>
                        ))}
                    </VStack>
                </Stack>
            )}
        </AwaitAPI>
    );
}
