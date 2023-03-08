import { Box, FormControl, FormHelperText, FormLabel, Icon, Spinner } from '@chakra-ui/react';
import { t } from 'i18next';
import { AiOutlinePlus } from 'react-icons/ai';
import { Artifact } from '../../../data/Artifact';
import { Project } from '../../../data/Project';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { useReload } from '../../../hooks/useReload';
import { components } from '../../../schemas/api';
import { concat, toLocalizedString } from '../../../schemas/generic';
import { getPrefered } from '../../../utils/preferedLanguage';
import { SendAPI } from '../SendAPI';
import { ArtifactUpload } from './Artifact';

interface IArtifactGroupUploadProps {
    artifactBlueprint: components['schemas']['ProjectArtifactBlueprintDto'];
    slotName: string;
    project: Project;
}

export function ArtifactGroupUpload(props: IArtifactGroupUploadProps) {
    const { border, bg } = useColorScheme();
    const reload = useReload();
    const {
        artifactBlueprint: { name, arity, description },
        project,
        slotName,
    } = props;

    const artifactsInSlot = project.artifacts.filter((artifact) => artifact.blueprintSlot === slotName);

    const newArtname = concat(project.name, ' (', toLocalizedString(name), ')');

    return (
        <SendAPI
            onSubmited={(id) => {
                project.artifacts.push(
                    new Artifact({
                        blueprintSlot: slotName,
                        name: newArtname,
                        id,
                        shards: [],
                    }),
                );
                reload();
            }}
            request={(api) => api.artifacts.create(newArtname, project.id, slotName)}
            value={null}
            repeatable
        >
            {(onSubmit, status) => (
                <FormControl>
                    <FormLabel>{getPrefered(toLocalizedString(name))}</FormLabel>
                    <FormHelperText mb={6}>
                        {getPrefered(toLocalizedString(description))} ({arity.min}
                        {arity.min !== arity.max && '-' + arity.max}×)
                    </FormHelperText>

                    {artifactsInSlot.map((artifact, key) => (
                        <ArtifactUpload artifact={artifact} key={key} {...props} />
                    ))}

                    <Box
                        borderColor={border}
                        bg={bg}
                        borderWidth={1}
                        borderRadius="md"
                        px={4}
                        py={2}
                        mb={2}
                        color={'gray.500'}
                        cursor="pointer"
                        onClick={onSubmit}
                    >
                        <Icon fontSize="1.2em">
                            <AiOutlinePlus />
                        </Icon>{' '}
                        {t('createProject.upload.newArtefact').toString()}
                        {status === 'sending' && <Spinner ml={6} size="sm" />}
                    </Box>
                </FormControl>
            )}
        </SendAPI>
    );
}
