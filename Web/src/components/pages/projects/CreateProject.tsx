import { Box, Stack } from '@chakra-ui/react';
import ChakraUIRenderer from 'chakra-ui-markdown-renderer';
import { t } from 'i18next';
import Markdown from 'react-markdown';
import { useParams } from 'react-router-dom';
import { Group } from '../../../data/Group';
import { useTitle } from '../../../utils/useTitle';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { Status } from '../../utils/Status';
import { ProjectBasicInfo } from './create/ProjectBasicInfo';

interface ICreateProjectProps {}

export function CreateProject(props: ICreateProjectProps) {
    const { id } = useParams();
    useTitle(t('createProject.title'));

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    return (
        <OutletOrChildren>
            <AwaitAPI
                request={(api) => api.groups.getById(id)}
                error={(resp) => {
                    return <Status statusCode={resp.response.status} log={resp.response.detail} embeded />;
                }}
            >
                {(group: Group) => (
                    <Stack spacing={4} m={6} direction="column">
                        <Box fontSize="xl" as="h2" lineHeight="tight" color="gray.500">
                            {t('createProject.title').toString()}
                        </Box>
                        <Box fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight">
                            {group.getName()}
                        </Box>
                        <Box fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight">
                            <Markdown components={ChakraUIRenderer()} skipHtml>
                                {group.getDescription()}
                            </Markdown>
                        </Box>
                        <hr />
                        <Box fontSize="xl" fontWeight="semibold" as="h3" lineHeight="tight">
                            {t('project.general').toString()}
                        </Box>
                        <ProjectBasicInfo groupId={id} />

                        {/*
                            - Genre
                            - Name
                            - Description
                            ? Visibility
                            ? ReleaseDate
                            Crew
                            Cast
                            Artifacts
                        */}
                    </Stack>
                )}
            </AwaitAPI>
        </OutletOrChildren>
    );
}
