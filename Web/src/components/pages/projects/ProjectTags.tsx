import { HStack, Tag, Text } from '@chakra-ui/react';
import { Link } from 'react-router-dom';
import { Project } from '../../../data/Project';
import { useAuthLink } from '../../../hooks/useAuthLink';
import { useColorScheme } from '../../../hooks/useColorScheme';
// import { VisibilityTag } from '../../utils/VisibilityTag';

interface IProjectTagsProps {
    project: Project;
}

export function ProjectTags({ project }: IProjectTagsProps) {
    const { bgDarker } = useColorScheme();

    return (
        <HStack spacing={2} mb={6}>
            {project.getGenre().length > 0 && <Text pr={8}>{project.getGenre()}</Text>}
            <Link to={useAuthLink(`/groups/${project.projectGroupId}`)}>
                <Tag bg={bgDarker}>{project.getGroupName()}</Tag>
            </Link>
            {/* <VisibilityTag visibility={project.visibility} /> */}
        </HStack>
    );
}
