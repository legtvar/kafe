import { FormControl, FormLabel, Input, Stack } from '@chakra-ui/react';
import { t } from 'i18next';
import { Project } from '../../../../data/Project';
import { useColorScheme } from '../../../../hooks/useColorScheme';
import { getPrefered } from '../../../../utils/preferedLanguage';
import { TextareaLimited } from '../../../utils/TextareaLimited';

interface IProjectBasicInfoProps {
    project?: Project;
}

export function ProjectBasicInfo({ project }: IProjectBasicInfoProps) {
    const { border, bg } = useColorScheme();

    return (
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
                            value={project && getPrefered(project.name, 'cs')}
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
                            value={project && getPrefered(project.name, 'en')}
                        />
                    </FormControl>
                </Stack>
            </FormControl>

            <FormControl>
                <FormLabel>{t('createProject.fields.genere').toString()}</FormLabel>
                <Stack direction={{ base: 'column', md: 'row' }}>
                    <FormControl id="name.cs">
                        <Input
                            type="text"
                            borderColor={border}
                            bg={bg}
                            placeholder={`${t('createProject.fields.genere').toString()} ${t(
                                'createProject.language.cs',
                            )}`}
                            value={project && getPrefered(project.genere, 'cs')}
                        />
                    </FormControl>

                    <FormControl id="name.en">
                        <Input
                            type="text"
                            borderColor={border}
                            bg={bg}
                            placeholder={`${t('createProject.fields.genere').toString()} ${t(
                                'createProject.language.en',
                            )}`}
                            value={project && getPrefered(project.genere, 'en')}
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
                            limit={250}
                            borderColor={border}
                            bg={bg}
                            value={project && getPrefered(project.description, 'cs')}
                        />
                    </FormControl>

                    <FormControl id="description.en">
                        <TextareaLimited
                            placeholder={`${t('createProject.fields.description').toString()} ${t(
                                'createProject.language.en',
                            )}`}
                            limit={250}
                            borderColor={border}
                            bg={bg}
                            value={project && getPrefered(project.description, 'en')}
                        />
                    </FormControl>
                </Stack>
            </FormControl>
        </Stack>
    );
}
