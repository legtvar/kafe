import { Button, FormControl, FormLabel, HStack, Input, Stack, useConst } from '@chakra-ui/react';
import useForceUpdate from 'use-force-update';
import { t } from 'i18next';
import { useNavigate } from 'react-router-dom';
import { API } from '../../../../api/API';
import { Project } from '../../../../data/Project';
import { useAuthLinkFunction } from '../../../../hooks/useAuthLink';
import { useColorScheme } from '../../../../hooks/useColorScheme';
import { HRIB } from '../../../../schemas/generic';
import { AuthorSelect } from '../../../utils/Authors/AuthorSelect';
import { LocalizedInput } from '../../../utils/LocalizedInput';
import { ProjectAuthorList } from '../../../utils/ProjectAuthorList';
import { SendAPI } from '../../../utils/SendAPI';
import { TextareaLimited } from '../../../utils/TextareaLimited';
import { ProjectBasicInfoForm } from './ProjectBasicInfoForm';
import { MateProjectBasicInfoForm } from './MateProjectBasicInfoForm';
import { currentOrganizationIdMapper } from '../../../../data/serialize/currentOrganizationIdMapper';
import { components } from '@/schemas/api';

interface IProjectBasicInfoProps {
    // Cannot be changed after initial draw
    project?: Project;
    groupId?: HRIB;
    validationSettings?: components['schemas']['ProjectValidationSettings']
    noSelfSubmit?: boolean;
}

export function ProjectBasicInfo(props: IProjectBasicInfoProps) {
    const { border, bg } = useColorScheme();
    const update = !!props.project;
    const project = useConst(
        props.project ||
            new Project({
                projectGroupId: props.groupId,
                validationSettings: props.validationSettings
            } as any),
    );
    const orgId = currentOrganizationIdMapper();

    const fu = useForceUpdate();
    const navigate = useNavigate();
    const authLink = useAuthLinkFunction();

    const forceUpdate = (any: any) => {
        fu();
    };

    const sendApiProps = update
        ? {
              onSubmited: (id: HRIB) => {
                  if (update) navigate(0);
                  else navigate(authLink(`/projects/${id}/edit`));
              },
              value: project!,
              request: (api: API, value: Project) => api.projects.update(value),
          }
        : {
              onSubmited: (id: HRIB) => {
                  navigate(authLink(`/projects/${id}/edit`));
              },
              value: project!,
              request: (api: API, value: Project) => api.projects.create(value),
          };

    return (
        <SendAPI {...sendApiProps}>
            {(onSubmit, status) =>
            orgId === 'mate-fimuni' ? (
                <MateProjectBasicInfoForm
                project={project}
                onSubmit={onSubmit}
                status={status}
                update={update}
                noSelfSubmit={props.noSelfSubmit}
                />
            ) : (
                <ProjectBasicInfoForm
                project={project}
                onSubmit={onSubmit}
                status={status}
                update={update}
                noSelfSubmit={props.noSelfSubmit}
                />
            )
            }
        </SendAPI>
    );
}
