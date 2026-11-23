import { Button, Checkbox, FormControl, FormHelperText, FormLabel, HStack, Input, RadioGroup, Radio, Stack, Textarea } from '@chakra-ui/react';
import useForceUpdate from 'use-force-update';
import { useTranslation } from 'react-i18next';
import { Project } from '../../../../data/Project';
import { useColorScheme } from '../../../../hooks/useColorScheme';
import { AuthorFlowDialog } from '../../../utils/Authors/AuthorFlowDialog';
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
    const { t } = useTranslation();
    const fu = useForceUpdate();
    const forceUpdate = (any: any) => {
        fu();
    };

    const isDropdownSelect = (project.genre === undefined || project.genreTags !== undefined);

    return (
        <Stack spacing={8} direction="column" mb={8}>
            <FormControl my={4}>
                <FormLabel>{t('createProject.fields.name').toString()}</FormLabel>
                <FormHelperText mb={"1rem"}>
                    {t('createProject.fields.nameHelp').toString()}
                </FormHelperText>
                <LocalizedInput
                    minLength={project.validationSettings?.minNameLength ?? undefined}
                    maxLength={project.validationSettings?.maxNameLength ?? undefined}
                    type="text"
                    borderColor={border}
                    bg={bg}
                    name="name"
                    placeholder={t('createProject.fields.name').toString()}
                    value={project.name}
                    onChange={(value) => forceUpdate(project.set('name', value))}
                    required={["en", ["cs", "sk"]]}
                    split
                />
            </FormControl>
            <FormControl my={4}>
                <FormLabel>{t('createProject.fields.genre').toString()}</FormLabel>
                {isDropdownSelect ?
                <>
                    <FormHelperText mb={"1rem"}>
                        {t(`createProject.fields.genreHelp`).toString()}
                    </FormHelperText>
                    <MultiSelect
                        options={genreManager.getOptions()}
                        value={project.genreTags}
                        onChange={value => genreManager.onChange(value, project, forceUpdate)}
                        create
                    />
                </> :
                <LocalizedInput
                    minLength={project.validationSettings?.minGenreLength ?? undefined}
                    maxLength={project.validationSettings?.maxGenreLength ?? undefined}
                    type="text"
                    borderColor={border}
                    bg={bg}
                    name="genre"
                    placeholder={t('createProject.fields.genre').toString()}
                    value={project.genre}
                    onChange={(value) => forceUpdate(project.set('genre', value))}
                />}
            </FormControl>
            <FormControl my={4}>
                <FormLabel>{t('createProject.fields.description').toString()}</FormLabel>
                <FormHelperText mb={"1rem"}>
                    {t('createProject.fields.descriptionHelp').toString()}
                </FormHelperText>
                <LocalizedInput
                    as={Textarea}
                    minLength={project.validationSettings?.minDescriptionLength ?? undefined}
                    maxLength={project.validationSettings?.maxDescriptionLength ?? undefined}
                    borderColor={border}
                    bg={bg}
                    name="description"
                    placeholder={t('createProject.fields.description').toString()}
                    value={project.description}
                    onChange={(value) => forceUpdate(project.set('description', value))}
                    required={["en", ["cs", "sk"]]}
                    split
                />
            </FormControl>
            <FormControl my={4}>
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
                    onRoleChange={(id, roles) =>
                        forceUpdate(
                            project.set(
                                'crew',
                                (project.crew || []).map((member) =>
                                    member.id === id ? { ...member, roles } : member,
                                ),
                            ),
                        )
                    }
                    isDropdownCrewList={isDropdownSelect}
                />
                <AuthorFlowDialog
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
                    isDropdownCrewSelect={isDropdownSelect}
                />
            </FormControl>
            <FormControl my={4}>
                <FormLabel>{t('createProject.fields.cast').toString()}</FormLabel>
                <FormHelperText mb={"1rem"}>
                    {t('createProject.fields.castHelp').toString()}
                </FormHelperText>
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
                    onRoleChange={(id, roles) =>
                        forceUpdate(
                            project.set(
                                'cast',
                                (project.cast || []).map((member) =>
                                    member.id === id ? { ...member, roles } : member,
                                ),
                            ),
                        )
                    }
                />
                <AuthorFlowDialog
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

            <FormControl my={4}>
                <FormLabel>{t('createProject.fields.aiUsageDeclaration.label').toString()}</FormLabel>
                <FormHelperText mb={"1rem"}>
                    {t('createProject.fields.aiUsageDeclaration.help').toString()}
                </FormHelperText>
                <RadioGroup
                    onChange={(value) => forceUpdate(project.set('aiUsageDeclaration', value))}
                    value={project.aiUsageDeclaration?.slice(0, 1)}>
                    <Stack direction='column'>
                        <Radio value='Y' borderColor="gray.500">
                            <i>{t('createProject.fields.aiUsageDeclaration.yes').toString()}</i>
                        </Radio>
                        <Radio value='N' borderColor="gray.500">
                            <i>{t('createProject.fields.aiUsageDeclaration.no').toString()}</i>
                        </Radio>
                    </Stack>
                </RadioGroup>
                {project.aiUsageDeclaration?.startsWith('Y') &&
                    <TextareaLimited
                        min={0}
                        max={200}
                        borderColor={border}
                        bg={bg}
                        mt={'1rem'}
                        placeholder={t('createProject.fields.aiUsageDeclaration.text').toString()}
                        value={project.aiUsageDeclaration?.slice(3)}
                        onChange={(event) => forceUpdate(project.set('aiUsageDeclaration', "Y: " + event.target.value.trim()))}
                    />
                }
            </FormControl>

            <FormControl my={4}>
                <FormLabel>{t('createProject.fields.isPublishedForUnlimitedTime.label').toString()}</FormLabel>
                <FormHelperText mb={"1rem"}>
                    {t('createProject.fields.isPublishedForUnlimitedTime.help').toString()}
                </FormHelperText>
                <Checkbox
                    borderColor="gray.500"
                    isChecked={project.isPublishedForUnlimitedTime}
                    onChange={(event) => forceUpdate(project.set('isPublishedForUnlimitedTime', event.target.checked))}
                >
                    {t('createProject.fields.isPublishedForUnlimitedTime.yes').toString()}
                </Checkbox>
            </FormControl>

            <FormControl my={4}>
                <FormLabel>{t('createProject.fields.hearAboutUs').toString()}</FormLabel>
                <FormHelperText mb={"1rem"}>
                    {t('createProject.fields.hearAboutUsHelp').toString()}
                </FormHelperText>
                <TextareaLimited
                    min={0}
                    max={120}
                    borderColor={border}
                    bg={bg}
                    placeholder={t('createProject.fields.hearAboutUs').toString()}
                    value={project.hearAboutUs}
                    onChange={(event) => forceUpdate(project.set('hearAboutUs', event.target.value.trim()))}
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
