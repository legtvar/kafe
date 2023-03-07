import { Box, FormControl, FormHelperText, FormLabel, Stack } from '@chakra-ui/react';
import { t } from 'i18next';
import { useState } from 'react';
import { useApi } from '../../../hooks/Caffeine';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { components } from '../../../schemas/api';
import { HRIB, toLocalizedString } from '../../../schemas/generic';
import { getPrefered } from '../../../utils/preferedLanguage';
import { Upload } from './Upload';

interface IUploadArtifactProps {
    artifactBlueprint: components['schemas']['ProjectArtifactBlueprintDto'];
    projectId: HRIB;
    artifactId?: HRIB;
}

export function UploadArtifact(props: IUploadArtifactProps) {
    const [artifactId, setArtifactId] = useState(props.artifactId || null);
    const { border, bg } = useColorScheme();
    const api = useApi();
    const {
        artifactBlueprint: { name, arity, /* slotName, */ description, shardBlueprints },
        projectId,
    } = props;

    const getArtifactId = async () => {
        if (artifactId) return artifactId;

        const result = await api.artifacts.create(toLocalizedString(name), projectId);

        if (result.status !== 200) {
            throw new Error(result.error as any);
        }
        setArtifactId(result.data);
        return result.data;
    };

    return (
        <FormControl>
            <FormLabel>{getPrefered(toLocalizedString(name))}</FormLabel>
            <FormHelperText mb={6}>
                {getPrefered(toLocalizedString(description))} ({arity.min}
                {arity.min !== arity.max && '-' + arity.max}×)
            </FormHelperText>
            <Stack direction="column" borderColor={border} bg={bg} borderWidth={1} borderRadius="md" p={4} spacing={12}>
                {shardBlueprints.map((shard) => (
                    <Box>
                        <FormLabel>{getPrefered(toLocalizedString(shard.name))}</FormLabel>
                        <FormHelperText mb={6}>
                            {getPrefered(toLocalizedString(shard.description))} ({arity.min}
                            {arity.min !== arity.max && '-' + arity.max}×)
                        </FormHelperText>
                        <Box w="100%" borderColor={border} bg={bg} borderWidth={1} borderRadius="md" py={4} px={6}>
                            <Upload
                                title={`${getPrefered(toLocalizedString(shard.name))} ${t(
                                    'generic.for',
                                ).toString()} ${getPrefered(toLocalizedString(name))}`.toLowerCase()}
                                projectId={projectId}
                                shardKind={shard.kind}
                                getArtifactId={getArtifactId}
                            />
                            {/* <Accordion allowToggle mx={-6} my={-4}>
                                <AccordionItem borderWidth="0 !important">
                                    <AccordionButton>
                                        <Box as="span" flex="1" textAlign="left">
                                            <Text fontWeight="bold" color={'gray.500'}>
                                                {t('createProject.upload.specs').toString()}
                                            </Text>
                                        </Box>
                                        <AccordionIcon />
                                    </AccordionButton>
                                    <AccordionPanel pb={4}>
                                        <UnorderedList listStyleType="none" ml={0} color={'gray.500'}>
                                            <ListItem>Formát (kodek) videa: H.264 (doporučený), MPEG-4 Part 2</ListItem>
                                            <ListItem>
                                                Formát (kodek) audia: WAV, FLAC, MP3 (bitrate u MP3 alespoň 192 kbps)
                                            </ListItem>
                                            <ListItem>Frame rate videa: 24 fps</ListItem>
                                            <ListItem>Titulky: anglické ve formátu SRT nebo ASS</ListItem>
                                            <ListItem>Kontejner: MP4 (doporučené), M4V, MKV</ListItem>
                                            <ListItem>Rozlišení: na šířku alespoň FullHD (t.j. 1920)</ListItem>
                                            <ListItem>Bitrate: 10 - 20 Mbps</ListItem>
                                            <ListItem>Hlasitost: max. -3dB</ListItem>
                                            <ListItem>Velikost: max. 2GB</ListItem>
                                        </UnorderedList>
                                    </AccordionPanel>
                                </AccordionItem>
                            </Accordion> */}
                        </Box>
                    </Box>
                ))}
            </Stack>
        </FormControl>
    );
}
