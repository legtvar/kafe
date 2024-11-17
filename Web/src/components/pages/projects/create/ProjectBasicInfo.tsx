import { Button, FormControl, FormLabel, HStack, Input, Stack, useConst, useForceUpdate } from '@chakra-ui/react';
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

interface IProjectBasicInfoProps {
    // Cannot be changed after initial draw
    project?: Project;
    groupId?: HRIB;
    noSelfSubmit?: boolean;
}

export function ProjectBasicInfo(props: IProjectBasicInfoProps) {
    const { border, bg } = useColorScheme();
    const update = !!props.project;
    const project = useConst(
        props.project ||
            new Project({
                projectGroupId: props.groupId,
            } as any),
    );
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
                  navigate(authLink(`/projects/${id}`));
              },
              value: project!,
              request: (api: API, value: Project) => api.projects.create(value),
          };

    return (
        <SendAPI {...sendApiProps}>
            {(onSubmit, status) => (
                <Stack spacing={8} direction="column" mb={8}>
                    <FormControl>
                        <FormLabel>{t('createProject.fields.name').toString()}</FormLabel>
                        <LocalizedInput
                            as={Input}
                            type="text"
                            borderColor={border}
                            bg={bg}
                            name="name"
                            placeholder={t('createProject.fields.name').toString()}
                            value={project.name}
                            onChange={(value) => forceUpdate(project.set('name', value))}
                        />
                    </FormControl>
                    <FormControl>
                        <FormLabel>{t('createProject.fields.genre').toString()}</FormLabel>
                        <LocalizedInput
                            as={Input}
                            type="text"
                            borderColor={border}
                            bg={bg}
                            name="genre"
                            placeholder={t('createProject.fields.genre').toString()}
                            value={project.genre}
                            onChange={(value) => forceUpdate(project.set('genre', value))}
                        />
                    </FormControl>
                    <FormControl>
                        <FormLabel>{t('createProject.fields.description').toString()}</FormLabel>
                        <LocalizedInput
                            as={TextareaLimited}
                            min={50}
                            max={200}
                            borderColor={border}
                            bg={bg}
                            name="description"
                            placeholder={t('createProject.fields.description').toString()}
                            value={project.description}
                            onChange={(value) => forceUpdate(project.set('description', value))}
                        />
                    </FormControl>
                    <FormControl py={12}>
                        <FormLabel>{t('createProject.fields.crew').toString()}</FormLabel>
                        <ProjectAuthorList
                            authors={project.crew || []}
                            editable
                            requestDetails
                            onRemove={(id) =>
                                forceUpdate(
                                    project.set(
                                        'crew',
                                        (project.crew || []).filter((member) => member.id !== id),
                                    ),
                                )
                            }
                        />
                        <AuthorSelect
                            onSelect={(id, roles) =>
                                forceUpdate(
                                    project.set('crew', [
                                        ...(project.crew || []),
                                        {
                                            id,
                                            roles,
                                        } as any,
                                    ]),
                                )
                            }
                        />
                    </FormControl>
                    <FormControl pb={12}>
                        <FormLabel>{t('createProject.fields.cast').toString()}</FormLabel>
                        <ProjectAuthorList
                            authors={project.cast || []}
                            editable
                            requestDetails
                            onRemove={(id) =>
                                forceUpdate(
                                    project.set(
                                        'cast',
                                        (project.cast || []).filter((member) => member.id !== id),
                                    ),
                                )
                            }
                        />
                        <AuthorSelect
                            onSelect={(id, roles) =>
                                forceUpdate(
                                    project.set('cast', [
                                        ...(project.cast || []),
                                        {
                                            id,
                                            roles,
                                        } as any,
                                    ]),
                                )
                            }
                        />
                    </FormControl>
                    {!props.noSelfSubmit && (
                        <HStack w="100%">
                            <Button
                                colorScheme="blue"
                                ml="auto"
                                isDisabled={status === 'sending' || status === 'ok'}
                                onClick={onSubmit}
                            >
                                {update
                                    ? t('createProject.button.update').toString()
                                    : t('createProject.button.create').toString()}
                            </Button>
                        </HStack>
                    )}
                </Stack>
            )}
        </SendAPI>
    );
}
