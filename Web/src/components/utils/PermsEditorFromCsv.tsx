import {
    Avatar,
    Badge,
    Box,
    Button,
    Checkbox,
    Flex,
    Input,
    InputGroup,
    Text,
    useColorModeValue,
    Modal,
    ModalOverlay,
    ModalContent,
    ModalHeader,
    ModalBody,
    ModalFooter,
    ModalCloseButton,
    useDisclosure,
} from '@chakra-ui/react';
import { t } from 'i18next';
import { useState } from 'react';
import { IoAdd, IoEarth, IoSaveOutline } from 'react-icons/io5';
import { Permission } from '../../schemas/generic';
import { useColorScheme } from '../../hooks/useColorScheme';
import { SendAPI } from './SendAPI';
import { useNavigate, useParams } from 'react-router-dom';
import { Status } from './Status';


export interface IPermsEditorModalProps {
    id: string
    options: Readonly<Array<Permission>>;
    initialPerms: Array<Permission>;
    readonly?: boolean;
}

export function PermsCsvEditorModal({ id, options, initialPerms, readonly }: IPermsEditorModalProps) {
    const { isOpen, onOpen, onClose } = useDisclosure();

    const borderColor = useColorModeValue('gray.300', 'gray.700');
    const navigate = useNavigate();
    const { border, bg } = useColorScheme();

    const [perms, setPerms] = useState<Permission[]>(initialPerms);
    const [csvFile, setCsvFile] = useState<File | null>(null);
    const [selectedFileName, setSelectedFileName] = useState<string>('');

    const permsNames: Record<Permission, string> = {
        read: t('perms.read').toString(),
        write: t('perms.write').toString(),
        inspect: t('perms.inspect').toString(),
        append: t('perms.append').toString(),
        review: t('perms.review').toString(),
    };

    const onValueChanges = (p: Permission[]) => {
        setPerms(p);
    };

    return (
        <>
            <Button
                colorScheme="blue"
                leftIcon={<IoAdd />}
                onClick={onOpen}
                mt={4}
                mb={4}
            >
                {t('perms.editCsv')}
            </Button>

            <Modal isOpen={isOpen} onClose={onClose} size="xl">
                <ModalOverlay />
                <ModalContent>
                    <ModalHeader>{t('perms.editCsv')}</ModalHeader>
                    <ModalCloseButton />

                    <ModalBody>
                        <Flex
                            direction="column"
                            gap={6}
                            py={2}
                            borderBottomWidth="1px"
                            borderBottomColor={borderColor}
                        >
                            <Flex>
                                <Button
                                    as="label"
                                    htmlFor="csv-upload"
                                    variant="outline"
                                    cursor="pointer"
                                    isDisabled={readonly}
                                    mr={4}
                                >
                                    {t('perms.uploadCsv')}
                                    <input
                                        id="csv-upload"
                                        type="file"
                                        accept=".csv"
                                        style={{ display: 'none' }}
                                        onChange={(e) => {
                                            const file = e.target.files?.[0] ?? null;
                                            setCsvFile(file);
                                            setSelectedFileName(file ? file.name : '');
                                        }}
                                    />
                                </Button>

                                <Box
                                    flex={1}
                                    bg={useColorModeValue('white', 'gray.900')}
                                    px={2}
                                    py={2}
                                    borderRadius="md"
                                >
                                    <Text
                                        color={
                                            selectedFileName
                                                ? useColorModeValue('black', 'white')
                                                : useColorModeValue('gray.400', 'whiteAlpha.400')
                                        }
                                    >
                                        {selectedFileName || t('perms.noCsv')}
                                    </Text>
                                </Box>
                            </Flex>

                            <Flex wrap="wrap">
                                {options.map((right, i) => (
                                    <Checkbox
                                        key={i}
                                        mr={4}
                                        mb={2}
                                        isDisabled={readonly}
                                        isChecked={perms.includes(right)}
                                        onChange={(event) => {
                                            if (event.target.checked) {
                                                onValueChanges([...perms, right]);
                                            } else {
                                                onValueChanges(perms.filter((r) => r !== right));
                                            }
                                        }}
                                    >
                                        {permsNames[right]}
                                    </Checkbox>
                                ))}
                            </Flex>
                        </Flex>
                    </ModalBody>

                    <ModalFooter>
                        <SendAPI
                            value={perms}
                            request={(api) =>
                                api.entities.perms.updateFromCsv(id, perms, csvFile!)
                            }
                            onSubmited={() => {
                                onClose();
                                navigate(0);
                            }}
                            repeatable={true}
                        >
                            {(savePerms, permsSaveStatus) => (
                                <Button
                                    leftIcon={<IoSaveOutline />}
                                    colorScheme="blue"
                                    isDisabled={readonly || !csvFile}
                                    onClick={() => savePerms()}
                                >
                                    {t('generic.save').toString()}
                                </Button>
                            )}
                        </SendAPI>
                    </ModalFooter>
                </ModalContent>
            </Modal>
        </>
    );
}
