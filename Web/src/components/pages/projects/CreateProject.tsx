import { Box, Stack } from '@chakra-ui/react';
import { t } from 'i18next';
import { useCallback } from 'react';
import { useParams } from 'react-router-dom';
import { Group } from '../../../data/Group';
import { useTitle } from '../../../utils/useTitle';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { ChakraMarkdown } from '../../utils/ChakraMarkdown';
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
                request={useCallback((api) => api.groups.getById(id), [id])}
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
                        <Box>
                            <ChakraMarkdown>{group.getDescription()}</ChakraMarkdown>
                        </Box>
                        <hr />
                        <Box fontSize="xl" fontWeight="semibold" as="h3" lineHeight="tight">
                            {t('project.general').toString()}
                        </Box>
                        <ProjectBasicInfo groupId={id} validationSettings={group.validationSettings}/>

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
