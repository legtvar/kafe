import {
    Box,
    Button,
    Center,
    Checkbox,
    Flex,
    HStack,
    Modal,
    ModalBody,
    ModalCloseButton,
    ModalContent,
    ModalFooter,
    ModalHeader,
    ModalOverlay,
    Radio,
    RadioGroup,
    Text,
    useColorModeValue,
} from '@chakra-ui/react';
import { t } from 'i18next';
import { useState } from 'react';
import { Group } from '../../../../data/Group';
import { PlaylistEntry } from '../../../../data/Playlist';
import { Project } from '../../../../data/Project';
import { useColorScheme } from '../../../../hooks/useColorScheme';
import { AwaitAPI } from '../../../utils/AwaitAPI';

const ALL_GROUPS = '_all';
const NO_PROJECT = '_none';

export interface IPlaylistAddNewFileWrapperProps {
    isOpen: boolean;
    onClose: (files: PlaylistEntry[]) => void;
    groups: Group[];
    projects: Project[];
}

export function PlaylistAddNewFileWrapper({ isOpen, onClose, groups, projects }: IPlaylistAddNewFileWrapperProps) {
    const [files, setFiles] = useState<PlaylistEntry[]>([]);
    const [selectedGroup, setSelectedGroup] = useState<string>(ALL_GROUPS);
    const [selectedProject, setSelectedProject] = useState<string>(NO_PROJECT);
    const { border, bg } = useColorScheme();
    const even = useColorModeValue('gray.100', 'gray.700');
    const odd = useColorModeValue('gray.200', 'gray.650');
    const evenSelected = useColorModeValue('blue.300', 'blue.600');
    const oddSelected = useColorModeValue('blue.400', 'blue.500');

    return (
        <Modal isOpen={isOpen} onClose={() => onClose(files)} size="full" scrollBehavior={'inside'}>
            <ModalOverlay />
            <ModalContent>
                <ModalHeader>{t('playlists.addNewFile.title')}</ModalHeader>
                <ModalCloseButton />
                <ModalBody as={Flex} direction="row">
                    <HStack align="stretch" justify="stretch" w="full">
                        <Box flex={1} overflowY="scroll" maxH="full">
                            <Text mb={4} ml={10} fontWeight="bold">
                                {t('playlists.addNewFile.groups')}
                            </Text>
                            <RadioGroup
                                value={selectedGroup}
                                onChange={(value) => {
                                    setSelectedGroup(value);

                                    // If the selected group is changed and the selected project is not in the new group, reset the selected project
                                    projects.find(
                                        (project) => project.id === selectedProject && project.projectGroupId === value,
                                    ) || setSelectedProject(NO_PROJECT);
                                }}
                            >
                                <HStack
                                    bg={selectedGroup === ALL_GROUPS ? oddSelected : odd}
                                    border={border}
                                    p={2}
                                    spacing={4}
                                >
                                    <Radio value={ALL_GROUPS} borderColor={border} flexGrow={1}>
                                        <Text flexGrow={1}>{t('playlists.addNewFile.allGroups')}</Text>
                                    </Radio>
                                </HStack>
                                {groups.map((group, i) => (
                                    <HStack
                                        key={i}
                                        bg={
                                            selectedGroup === group.id
                                                ? i % 2 === 0
                                                    ? evenSelected
                                                    : oddSelected
                                                : i % 2 === 0
                                                ? even
                                                : odd
                                        }
                                        border={border}
                                        p={2}
                                        spacing={4}
                                    >
                                        <Radio value={group.id} borderColor={border} flexGrow={1}>
                                            <Text flexGrow={1}>{group.getName()}</Text>
                                        </Radio>
                                    </HStack>
                                ))}
                            </RadioGroup>
                        </Box>
                        <Box flex={1} overflowY="scroll" maxH="full">
                            <Text mb={4} ml={10} fontWeight="bold">
                                {t('playlists.addNewFile.projects')}
                            </Text>
                            <RadioGroup
                                value={selectedProject || NO_PROJECT}
                                onChange={(value) => setSelectedProject(value)}
                            >
                                {projects
                                    .filter(
                                        (project) =>
                                            selectedGroup === ALL_GROUPS || project.projectGroupId === selectedGroup,
                                    )
                                    .map((project, i) => (
                                        <HStack
                                            key={i}
                                            bg={
                                                selectedProject === project.id
                                                    ? i % 2 === 0
                                                        ? evenSelected
                                                        : oddSelected
                                                    : i % 2 === 0
                                                    ? even
                                                    : odd
                                            }
                                            border={border}
                                            p={2}
                                            spacing={4}
                                        >
                                            <Radio value={project.id} borderColor={border} flexGrow={1}>
                                                <Text flexGrow={1}>{project.getName()}</Text>
                                            </Radio>
                                        </HStack>
                                    ))}
                            </RadioGroup>
                        </Box>
                        <Box flex={1} overflowY="scroll">
                            <Text mb={4} ml={10} fontWeight="bold">
                                {t('playlists.addNewFile.artifacts')}
                            </Text>
                            {selectedProject === NO_PROJECT ? (
                                <Center fontStyle="italic">
                                    <Text>{t('playlists.addNewFile.noProjectSelected')}</Text>
                                </Center>
                            ) : (
                                <AwaitAPI
                                    key={selectedProject}
                                    request={(api) => api.projects.getById(selectedProject)}
                                    error={(error) => <Text>{error.message}</Text>}
                                >
                                    {(project: Project) => (
                                        <>
                                            {project.artifacts.map((artifact, i) => (
                                                <HStack
                                                    key={i}
                                                    bg={
                                                        files.some((file) => file.id === artifact.id)
                                                            ? i % 2 === 0
                                                                ? evenSelected
                                                                : oddSelected
                                                            : i % 2 === 0
                                                            ? even
                                                            : odd
                                                    }
                                                    border={border}
                                                    p={2}
                                                    spacing={4}
                                                >
                                                    <Checkbox
                                                        isChecked={files.some((file) => file.id === artifact.id)}
                                                        onChange={(e) => {
                                                            if (e.target.checked) {
                                                                setFiles([...files, artifact]);
                                                            } else {
                                                                setFiles(
                                                                    files.filter((file) => file.id !== artifact.id),
                                                                );
                                                            }
                                                        }}
                                                        borderColor={border}
                                                        flexGrow={1}
                                                    >
                                                        <Text flexGrow={1}>{artifact.getName()}</Text>
                                                    </Checkbox>
                                                </HStack>
                                            ))}
                                        </>
                                    )}
                                </AwaitAPI>
                            )}
                        </Box>
                    </HStack>
                </ModalBody>

                <ModalFooter>
                    <Text mr={5}>{t('playlists.addNewFile.selectedFiles', { count: files.length })}</Text>
                    <Button
                        colorScheme="blue"
                        mr={3}
                        onClick={() => {
                            setFiles([]);
                            onClose(files);
                        }}
                    >
                        {t('playlists.addNewFile.add')}
                    </Button>
                    <Button
                        variant="ghost"
                        onClick={() => {
                            setFiles([]);
                            onClose([]);
                        }}
                    >
                        {t('generic.cancel')}
                    </Button>
                </ModalFooter>
            </ModalContent>
        </Modal>
    );
}
