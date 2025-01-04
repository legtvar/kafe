import {
    Accordion,
    AccordionButton,
    AccordionItem,
    AccordionPanel,
    Box,
    FormHelperText,
    FormLabel,
    Icon,
    Text,
} from '@chakra-ui/react';
import { AiOutlinePlus } from 'react-icons/ai';
import { Artifact } from '../../../data/Artifact';
import { Project } from '../../../data/Project';
import { Shard } from '../../../data/Shard';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { useReload } from '../../../hooks/useReload';
import { components } from '../../../schemas/api';
import { toLocalizedString } from '../../../schemas/generic';
import { getPrefered } from '../../../utils/preferedLanguage';
import { FileIcon } from '../FileIcon';
import { Upload } from './Upload';

interface IShardGroupUploadProps {
    project: Project;
    shardBlueprint: components['schemas']['ProjectArtifactShardBlueprintDto'];
    artifact: Artifact;
    kind: components['schemas']['ShardKind'];
}

export function ShardGroupUpload(props: IShardGroupUploadProps) {
    const { border, bg, bgPage } = useColorScheme();
    const reload = useReload();
    const {
        shardBlueprint: { name, arity, description },
        project,
        artifact,
        kind,
    } = props;

    const filesInGroup = artifact.shards.filter((shard) => shard.kind === kind);

    return (
        <Box borderColor={border} bg={bgPage} borderWidth={1} borderRadius="md" p={4}>
            <FormLabel>{getPrefered(toLocalizedString(name))}</FormLabel>
            <FormHelperText mb={6}>
                {getPrefered(toLocalizedString(description))} ({arity.min}
                {arity.min !== arity.max && '-' + arity.max}×)
            </FormHelperText>
            {filesInGroup.map((shard, i) => (
                <Box
                    w="100%"
                    borderColor={border}
                    bg={bg}
                    borderWidth={1}
                    borderRadius="md"
                    py={2}
                    px={4}
                    mb={2}
                    key={i}
                >
                    <Text>
                        <Icon fontSize="1.2em">
                            <FileIcon kind={kind} />
                        </Icon>{' '}
                        {shard.id}
                    </Text>
                </Box>
            ))}
            {filesInGroup.length < arity.max && (
                <Box w="100%" borderColor={border} bg={bg} borderWidth={1} borderRadius="md" py={4} px={4}>
                    <Accordion allowToggle m={-4}>
                        <AccordionItem borderWidth="0 !important">
                            <AccordionButton color={'gray.500'}>
                                <Box as="span" flex="1" textAlign="left">
                                    <Icon fontSize="1.2em">
                                        <AiOutlinePlus />
                                    </Icon>{' '}
                                    Přidat nový soubor
                                </Box>
                            </AccordionButton>
                            <AccordionPanel pb={4}>
                                <Upload
                                    title={getPrefered(toLocalizedString(name)).toLowerCase()}
                                    projectId={project.id}
                                    shardKind={kind as any}
                                    artifactId={artifact.id}
                                    onUploaded={(id) => {
                                        artifact.shards.push(
                                            new Shard({
                                                id,
                                                kind,
                                            }),
                                        );
                                        reload();
                                    }}
                                    repeatable
                                />
                            </AccordionPanel>
                        </AccordionItem>
                    </Accordion>
                </Box>
            )}
        </Box>
    );
}
