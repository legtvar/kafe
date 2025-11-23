import { Button, FormControl, FormLabel, HStack, Input, Stack } from '@chakra-ui/react';
import useForceUpdate from 'use-force-update';
import { t } from 'i18next';
import { Project } from '../../../../data/Project';
import { useColorScheme } from '../../../../hooks/useColorScheme';
import { AuthorSelect } from '../../../utils/Authors/AuthorSelect';
import { LocalizedInput } from '../../../utils/LocalizedInput';
import { ProjectAuthorList } from '../../../utils/ProjectAuthorList';
import { TextareaLimited } from '../../../utils/TextareaLimited';

interface ProjectBasicInfoFormProps {
    project: Project;
    onSubmit: () => void;
    status: string;
    update: boolean;
    noSelfSubmit?: boolean;
}

export function MateProjectBasicInfoForm({ project, onSubmit, status, update, noSelfSubmit }: ProjectBasicInfoFormProps) {
    const { border, bg } = useColorScheme();
    const fu = useForceUpdate();
    const forceUpdate = (any: any) => {
        fu();
    };

    return (
        <Stack spacing={8} direction="column" mb={8}>
            <FormControl>
                <FormLabel>{t('createProject.fields.nameMate').toString()}</FormLabel>
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
                <FormLabel>{t('createProject.fields.description').toString()}</FormLabel>
                <LocalizedInput
                    as={TextareaLimited}
                    min={50}
                    max={10000}
                    borderColor={border}
                    bg={bg}
                    name="description"
                    placeholder={t('createProject.fields.description').toString()}
                    value={project.description}
                    onChange={(value) => forceUpdate(project.set('description', value))}
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
