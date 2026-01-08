import { Button, FormControl, FormHelperText, FormLabel, HStack, Stack } from '@chakra-ui/react';
import useForceUpdate from 'use-force-update';
import { t } from 'i18next';
import { Project } from '../../../../data/Project';
import { useColorScheme } from '../../../../hooks/useColorScheme';
import { AuthorSelect } from '../../../utils/Authors/AuthorSelect';
import { LocalizedInput } from '../../../utils/LocalizedInput';
import { ProjectAuthorList } from '../../../utils/ProjectAuthorList';
import { TextareaLimited } from '../../../utils/TextareaLimited';
import { MultiSelect } from 'chakra-multiselect';
import genreManager from '../../../../utils/managers/genreManager';

interface ProjectBasicInfoFormProps {
    project: Project;
    onSubmit: () => void;
    status: string;
    update: boolean;
    noSelfSubmit?: boolean;
}

export function ProjectBasicInfoForm({ project, onSubmit, status, update, noSelfSubmit }: ProjectBasicInfoFormProps) {
    const { border, bg } = useColorScheme();
    const fu = useForceUpdate();
    const forceUpdate = (any: any) => {
        fu();
    };

    return (
        <Stack spacing={8} direction="column" mb={8}>
            <FormControl>
                <FormLabel>{t('createProject.fields.name').toString()}</FormLabel>
                <LocalizedInput
                    as={TextareaLimited}
                    min={project.validationSettings?.minNameLength ?? undefined}
                    max={project.validationSettings?.maxNameLength ?? undefined}
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
                {!project.genre || project.genre.tags ?
                <>
                    <FormHelperText mb={"1rem"}>
                        {t(`createProject.fields.genreHelp`).toString()}
                    </FormHelperText>
                    <MultiSelect
                        options={genreManager.getOptions()}
                        value={genreManager.getValue(project)}
                        onChange={value => genreManager.onChange(value, project, forceUpdate)}
                        create
                    />
                </> :
                <LocalizedInput
                    as={TextareaLimited}
                    min={project.validationSettings?.minGenreLength ?? undefined}
                    max={project.validationSettings?.maxGenreLength ?? undefined}
                    type="text"
                    borderColor={border}
                    bg={bg}
                    name="genre"
                    placeholder={t('createProject.fields.genre').toString()}
                    value={project.genre}
                    onChange={(value) => forceUpdate(project.set('genre', value))}
                />}
            </FormControl>
            <FormControl>
                <FormLabel>{t('createProject.fields.description').toString()}</FormLabel>
                <LocalizedInput
                    as={TextareaLimited}
                    min={project.validationSettings?.minDescriptionLength ?? undefined}
                    max={project.validationSettings?.maxDescriptionLength ?? undefined}
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
                <FormHelperText mb={"1rem"}>
                    {t(`createProject.fields.crewHelp`).toString()}
                </FormHelperText>
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
                    isCrewList={project.genre === undefined || project.genre.tags !== undefined}
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
                    isCrewSelect={project.genre === undefined || project.genre.tags !== undefined}
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
            {!noSelfSubmit && (
                <HStack w="100%">
                    <Button
                        colorScheme="blue"
                        ml="auto"
                        isDisabled={status === 'sending' || status === 'ok'}
                        onClick={onSubmit}
                    >
                        {update ? t('createProject.button.update') : t('createProject.button.create')}
                    </Button>
                </HStack>
            )}
        </Stack>
    );
}
