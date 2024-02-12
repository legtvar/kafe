import {
    Accordion,
    AccordionButton,
    AccordionIcon,
    AccordionItem,
    AccordionPanel,
    AlertDialog,
    AlertDialogBody,
    AlertDialogContent,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogOverlay,
    Box,
    Button,
    SimpleGrid,
    Text,
    useDisclosure,
} from '@chakra-ui/react';
import { t } from 'i18next';
import { createRef } from 'react';
import { IoTrashOutline } from 'react-icons/io5';
import { useNavigate } from 'react-router-dom';
import { Artifact } from '../../../data/Artifact';
import { Project } from '../../../data/Project';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { components } from '../../../schemas/api';
import { SendAPI } from '../SendAPI';
import { ShardGroupUpload } from './ShardGroup';

interface IArtifactUploadProps {
    project: Project;
    artifactBlueprint: components['schemas']['ProjectArtifactBlueprintDto'];
    artifact: Artifact;
}

export function ArtifactUpload(props: IArtifactUploadProps) {
    const { border, bg } = useColorScheme();
    const { isOpen, onOpen, onClose } = useDisclosure();
    const {
        artifactBlueprint: { shardBlueprints },
        artifact,
        project,
    } = props;
    const cancelRef = createRef<HTMLButtonElement>();
    const navigate = useNavigate();

    return (
        <SendAPI
            onSubmited={(id) => {
                navigate(0);
            }}
            request={(api) => {
                project.set(
                    'artifacts',
                    project.artifacts.filter((a) => a.id !== artifact.id),
                );
                return api.projects.update(project);
            }}
            value={null}
            repeatable
        >
            {(onSubmit, status) => (
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
                                    {Object.entries(shardBlueprints).map(([kind, shard], i) => (
                                        <ShardGroupUpload
                                            kind={kind as any}
                                            shardBlueprint={shard!}
                                            key={i}
                                            {...props}
                                        />
                                    ))}
                                </SimpleGrid>
                                <Button mt={4} colorScheme="red" leftIcon={<IoTrashOutline />} onClick={onOpen}>
                                    {t('artifact.delete').toString()}
                                </Button>

                                <AlertDialog isOpen={isOpen} leastDestructiveRef={cancelRef} onClose={onClose}>
                                    <AlertDialogOverlay>
                                        <AlertDialogContent>
                                            <AlertDialogHeader fontSize="lg" fontWeight="bold">
                                                {t('artifact.delete').toString()}
                                            </AlertDialogHeader>

                                            <AlertDialogBody>{t('artifact.deleteWarn').toString()}</AlertDialogBody>

                                            <AlertDialogFooter>
                                                <Button ref={cancelRef} onClick={onClose}>
                                                    {t('generic.cancel').toString()}
                                                </Button>
                                                <Button
                                                    colorScheme="red"
                                                    onClick={() => {
                                                        onSubmit();
                                                        onClose();
                                                    }}
                                                    ml={3}
                                                >
                                                    {t('artifact.delete').toString()}
                                                </Button>
                                            </AlertDialogFooter>
                                        </AlertDialogContent>
                                    </AlertDialogOverlay>
                                </AlertDialog>
                            </AccordionPanel>
                        </AccordionItem>
                    </Accordion>
                </Box>
            )}
        </SendAPI>
    );
}
