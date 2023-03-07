import { Button, FormControl, FormLabel, HStack, Input, Stack, useConst, useForceUpdate } from '@chakra-ui/react';
import { t } from 'i18next';
import { useNavigate } from 'react-router-dom';
import { Project } from '../../../../data/Project';
import { useColorScheme } from '../../../../hooks/useColorScheme';
import { HRIB } from '../../../../schemas/generic';
import { getPrefered } from '../../../../utils/preferedLanguage';
import { AuthorSelect } from '../../../utils/Authors/AuthorSelect';
import { ProjectAuthorList } from '../../../utils/ProjectAuthorList';
import { SendAPI } from '../../../utils/SendAPI';
import { TextareaLimited } from '../../../utils/TextareaLimited';

interface IProjectBasicInfoProps {
    // Cannot be changed after initial draw
    project?: Project;
    groupId?: HRIB;
}

export function ProjectBasicInfo(props: IProjectBasicInfoProps) {
    const { border, bg } = useColorScheme();
    // const update = !!props.project; // Todo accept updating the project
    const project = useConst(
        props.project ||
            new Project({
                projectGroupId: props.groupId,
            } as any),
    );
    const fu = useForceUpdate();
    const navigate = useNavigate();

    const forceUpdate = (any: any) => {
        fu();
    };

    return (
        <SendAPI
            onSubmited={(id) => {
                navigate(`/auth/project/${id}`);
            }}
            value={project!}
            request={(api, value) => api.projects.create(value)}
        >
            {(onSubmit, status) => (
                <Stack spacing={8} direction="column">
                    <FormControl>
                        <FormLabel>{t('createProject.fields.name').toString()}</FormLabel>
                        <Stack direction={{ base: 'column', md: 'row' }}>
                            <FormControl id="name.cs">
                                <Input
                                    type="text"
                                    borderColor={border}
                                    bg={bg}
                                    placeholder={`${t('createProject.fields.name').toString()} ${t(
                                        'createProject.language.cs',
                                    )}`}
                                    defaultValue={getPrefered(project.name, 'cs')}
                                    onChange={(event) =>
                                        forceUpdate(
                                            project.set('name', {
                                                ...project.name,
                                                cs: event.target.value,
                                            }),
                                        )
                                    }
                                />
                            </FormControl>

                            <FormControl id="name.en">
                                <Input
                                    type="text"
                                    borderColor={border}
                                    bg={bg}
                                    placeholder={`${t('createProject.fields.name').toString()} ${t(
                                        'createProject.language.en',
                                    )}`}
                                    defaultValue={getPrefered(project.name, 'en')}
                                    onChange={(event) =>
                                        forceUpdate(
                                            project.set('name', {
                                                ...project.name,
                                                en: event.target.value,
                                            }),
                                        )
                                    }
                                />
                            </FormControl>
                        </Stack>
                    </FormControl>

                    <FormControl>
                        <FormLabel>{t('createProject.fields.genere').toString()}</FormLabel>
                        <Stack direction={{ base: 'column', md: 'row' }}>
                            <FormControl id="genere.cs">
                                <Input
                                    type="text"
                                    borderColor={border}
                                    bg={bg}
                                    placeholder={`${t('createProject.fields.genere').toString()} ${t(
                                        'createProject.language.cs',
                                    )}`}
                                    defaultValue={getPrefered(project.genere, 'cs')}
                                    onChange={(event) =>
                                        forceUpdate(
                                            project.set('genere', {
                                                ...project.genere,
                                                cs: event.target.value,
                                            }),
                                        )
                                    }
                                />
                            </FormControl>

                            <FormControl id="genere.en">
                                <Input
                                    type="text"
                                    borderColor={border}
                                    bg={bg}
                                    placeholder={`${t('createProject.fields.genere').toString()} ${t(
                                        'createProject.language.en',
                                    )}`}
                                    defaultValue={getPrefered(project.genere, 'en')}
                                    onChange={(event) =>
                                        forceUpdate(
                                            project.set('genere', {
                                                ...project.genere,
                                                en: event.target.value,
                                            }),
                                        )
                                    }
                                />
                            </FormControl>
                        </Stack>
                    </FormControl>

                    <FormControl>
                        <FormLabel>{t('createProject.fields.description').toString()}</FormLabel>
                        <Stack direction={{ base: 'column', md: 'row' }}>
                            <FormControl id="description.cs">
                                <TextareaLimited
                                    placeholder={`${t('createProject.fields.description').toString()} ${t(
                                        'createProject.language.cs',
                                    )}`}
                                    min={50}
                                    max={200}
                                    borderColor={border}
                                    bg={bg}
                                    defaultValue={getPrefered(project.description, 'cs')}
                                    onChange={(event) =>
                                        forceUpdate(
                                            project.set('description', {
                                                ...project.description,
                                                cs: event.target.value,
                                            }),
                                        )
                                    }
                                />
                            </FormControl>

                            <FormControl id="description.en">
                                <TextareaLimited
                                    placeholder={`${t('createProject.fields.description').toString()} ${t(
                                        'createProject.language.en',
                                    )}`}
                                    min={50}
                                    max={200}
                                    borderColor={border}
                                    bg={bg}
                                    defaultValue={getPrefered(project.description, 'en')}
                                    onChange={(event) =>
                                        forceUpdate(
                                            project.set('description', {
                                                ...project.description,
                                                en: event.target.value,
                                            }),
                                        )
                                    }
                                />
                            </FormControl>
                        </Stack>
                    </FormControl>

                    <FormControl pb={12}>
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
                    <HStack w="100%" pb={12}>
                        <Button
                            colorScheme="blue"
                            ml="auto"
                            isDisabled={status === 'sending' || status === 'ok'}
                            onClick={onSubmit}
                        >
                            Vytvořit projekt
                        </Button>
                    </HStack>
                </Stack>
            )}
        </SendAPI>
    );
}
