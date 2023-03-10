import { Box, Stack } from '@chakra-ui/react';
import { t } from 'i18next';
import { useParams } from 'react-router-dom';
import { Group } from '../../../data/Group';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { Status } from '../../utils/Status';
import { IntroText } from './create/IntroText';
import { ProjectBasicInfo } from './create/ProjectBasicInfo';

interface ICreateProjectProps {}

export function CreateProject(props: ICreateProjectProps) {
    const { id } = useParams();

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    return (
        <OutletOrChildren>
            <AwaitAPI request={(api) => api.groups.getById(id)} error={<Status statusCode={404} embeded />}>
                {(group: Group) => (
                    <Stack spacing={4} m={6} direction="column">
                        <Box fontSize="xl" as="h2" lineHeight="tight" color="gray.500" isTruncated>
                            {t('createProject.title').toString()}
                        </Box>
                        <Box fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" isTruncated>
                            {group.getName()}
                        </Box>

                        <IntroText groupName={group.getName()} displayDetails />
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
