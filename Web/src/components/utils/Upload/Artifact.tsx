import {
    Accordion,
    AccordionButton,
    AccordionIcon,
    AccordionItem,
    AccordionPanel,
    Box,
    SimpleGrid,
    Text,
} from '@chakra-ui/react';
import { t } from 'i18next';
import { Artifact } from '../../../data/Artifact';
import { Project } from '../../../data/Project';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { components } from '../../../schemas/api';
import { ShardGroupUpload } from './ShardGroup';

interface IArtifactUploadProps {
    project: Project;
    artifactBlueprint: components['schemas']['ProjectArtifactBlueprintDto'];
    artifact: Artifact;
}

export function ArtifactUpload(props: IArtifactUploadProps) {
    const { border, bg } = useColorScheme();
    const {
        artifactBlueprint: { shardBlueprints },
        artifact,
    } = props;

    return (
        <Box borderColor={border} bg={bg} borderWidth={1} borderRadius="md" p={4} mb={2}>
            <Accordion allowToggle m={-4}>
                <AccordionItem borderWidth="0 !important">
                    <AccordionButton>
                        <Box as="span" flex="1" textAlign="left">
                            {artifact.getName()}{' '}
                            <Text as="span" pl={4} color={'gray.500'}>
                                ({artifact.shards.length} {t('generic.files').toString()})
                            </Text>
                        </Box>
                        <AccordionIcon />
                    </AccordionButton>
                    <AccordionPanel pb={4}>
                        <SimpleGrid columns={{ base: 1, lg: 2, xl: 3 }} spacing={4} w="100%">
                            {Object.entries(shardBlueprints).map(([kind, shard]) => (
                                <ShardGroupUpload kind={kind as any} shardBlueprint={shard!} {...props} />
                            ))}
                        </SimpleGrid>
                    </AccordionPanel>
                </AccordionItem>
            </Accordion>
        </Box>
    );
}
