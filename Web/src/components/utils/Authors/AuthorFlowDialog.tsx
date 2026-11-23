import {
    Box,
    Button,
    FormControl,
    FormLabel,
    HStack,
    Input,
    InputGroup,
    InputLeftElement,
    Modal,
    ModalBody,
    ModalCloseButton,
    ModalContent,
    ModalFooter,
    ModalHeader,
    ModalOverlay,
    Text,
    Textarea,
    useDisclosure,
    VStack,
} from '@chakra-ui/react';
import { MultiSelect } from 'chakra-multiselect';
import { t } from 'i18next';
import { RefObject, useCallback, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import useForceUpdate from 'use-force-update';
import { AiOutlinePlus, AiOutlineSearch } from 'react-icons/ai';
import { Author } from '../../../data/Author';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { HRIB } from '../../../schemas/generic';
import crewRolesManager from '../../../utils/managers/crewRolesManager';
import { AwaitAPI } from '../AwaitAPI';
import { LocalizedInput } from '../LocalizedInput';
import { SendAPI } from '../SendAPI';
import { TagInput } from './TagInput';

export type AuthorFlowStatus = 'query' | 'new' | 'role';

interface IAuthorFlowDialogProps {
    initialStatus?: AuthorFlowStatus;
    isOpen?: boolean;
    isDropdownCrewSelect?: boolean;
    author?: Author;
    canEdit?: boolean;
    roles?: string[];
    onSelect?: (id: HRIB, roles: string[]) => void;
    onSaved?: (author: Author) => void;
    onRolesSaved?: (roles: string[]) => void;
    onClose?: () => void;
}

export function AuthorFlowDialog({
    initialStatus,
    isOpen: controlledIsOpen,
    isDropdownCrewSelect,
    author: initialAuthor,
    canEdit = true,
    roles: initialRoles,
    onSelect,
    onSaved,
    onRolesSaved,
    onClose,
}: IAuthorFlowDialogProps) {
    const { isOpen, onOpen, onClose: closeDisclosure } = useDisclosure();
    const [status, setStatus] = useState<AuthorFlowStatus>(initialStatus ?? (initialAuthor ? 'new' : 'query'));
    const [author, setAuthor] = useState<Author | null>(initialAuthor ? new Author(initialAuthor) : null);
    const [name, setName] = useState('');
    const [key, setKey] = useState(Math.random());
    const [roles, setRoles] = useState(initialRoles ?? []);
    const initialRef = useRef<HTMLInputElement>(null);

    const close = () => {
        if (!initialAuthor) {
            setName('');
            setAuthor(null);
            setStatus('query');
            setKey(Math.random());
        }

        closeDisclosure();
        onClose?.();
    };

    return (
        <>
            {!initialAuthor && !initialStatus && (
                <Button onClick={onOpen} mt={4}>
                    {t('authorSelect.button').toString()}
                </Button>
            )}
            <Modal initialFocusRef={initialRef} isOpen={controlledIsOpen ?? isOpen} onClose={close}>
                <ModalOverlay />
                {status === 'query' && (
                    <AuthorQueryModal
                        initialRef={initialRef}
                        name={name}
                        onNameChange={setName}
                        onSelect={(selectedAuthor) => {
                            setAuthor(selectedAuthor);
                            setStatus('role');
                        }}
                        onCreate={() => {
                            setAuthor(new Author({ id: '', name, globalPermissions: 'none' }));
                            setStatus('new');
                        }}
                        onClose={close}
                    />
                )}
                {status === 'new' && author && (
                    <AuthorNewModal
                        key={key}
                        author={author}
                        onChange={setAuthor}
                        isExisting={!!initialAuthor}
                        canEdit={canEdit}
                        onSaved={(savedAuthor, id) => {
                            savedAuthor.id = id;
                            setAuthor(savedAuthor);
                            if (initialAuthor) {
                                onSaved?.(savedAuthor);
                                close();
                            } else {
                                setStatus('role');
                            }
                        }}
                        onClose={close}
                    />
                )}
                {status === 'role' && (
                    <AuthorRoleModal
                        roles={roles}
                        isDropdownCrewSelect={isDropdownCrewSelect}
                        onSaved={(selectedRoles) => {
                            setRoles(selectedRoles);
                            if (author) {
                                onSelect?.(author.id, selectedRoles);
                            }

                            onRolesSaved?.(selectedRoles);
                            close();
                        }}
                        onClose={close}
                    />
                )}
            </Modal>
        </>
    );
}

interface IAuthorQueryModalProps {
    initialRef: RefObject<HTMLInputElement>;
    name: string;
    onNameChange: (name: string) => void;
    onSelect: (author: Author) => void;
    onCreate: () => void;
    onClose: () => void;
}

function AuthorQueryModal({ initialRef, name, onNameChange, onSelect, onCreate, onClose }: IAuthorQueryModalProps) {
    const { bg, lighten } = useColorScheme();

    return (
        <ModalContent>
            <ModalHeader>{t('authorSelect.button').toString()}</ModalHeader>
            <ModalCloseButton />
            <ModalBody pb={6}>
                <FormControl>
                    <InputGroup>
                        <InputLeftElement>
                            <AiOutlineSearch />
                        </InputLeftElement>
                        <Input
                            ref={initialRef}
                            value={name}
                            onChange={(event) => onNameChange(event.target.value)}
                            placeholder={t('authorSelect.name').toString()}
                        />
                    </InputGroup>
                </FormControl>
                <AwaitAPI request={useCallback((api) => api.authors.getAll(), [])}>
                    {(authors: Author[]) => name.length > 0 ? (
                        <VStack alignItems="stretch" spacing={0} mx={-6} mt={4}>
                            {authors
                                .filter((author) => author.name.toLowerCase().includes(name.toLowerCase()))
                                .sort((a, b) => a.name.localeCompare(b.name))
                                .slice(0, 5)
                                .map((author) => (
                                    <Box
                                        px={8}
                                        py={4}
                                        cursor="pointer"
                                        _hover={{ background: bg }}
                                        key={author.id}
                                        onClick={() => onSelect(author)}
                                    >
                                        {author.name}
                                    </Box>
                                ))}
                            <HStack
                                px={8}
                                py={4}
                                cursor="pointer"
                                color={lighten}
                                _hover={{ background: bg }}
                                onClick={onCreate}
                            >
                                <AiOutlinePlus />
                                <Text>{t('authorSelect.new').toString()}</Text>
                            </HStack>
                        </VStack>
                    ) : <></>}
                </AwaitAPI>
            </ModalBody>
            <ModalFooter>
                <Button onClick={onClose}>{t('generic.cancel').toString()}</Button>
            </ModalFooter>
        </ModalContent>
    );
}

interface IAuthorNewModalProps {
    author: Author;
    onChange: (author: Author) => void;
    isExisting: boolean;
    canEdit: boolean;
    onSaved: (author: Author, id: HRIB) => void;
    onClose: () => void;
}

function AuthorNewModal({ author, onChange, isExisting, canEdit, onSaved, onClose }: IAuthorNewModalProps) {
    const { t } = useTranslation();
    const forceUpdate = useForceUpdate();

    return (
        <ModalContent>
            <ModalHeader>{isExisting ? author.name : t('authorSelect.button').toString()}</ModalHeader>
            <ModalCloseButton />
            <ModalBody pb={6}>
                {canEdit ? (
                    <>
                        <AuthorField author={author} field="name" onChange={onChange} />
                        <AuthorField author={author} field="uco" onChange={onChange} />
                        <AuthorField author={author} field="email" onChange={onChange} />
                        <AuthorField author={author} field="phone" onChange={onChange} />
                        <FormControl>
                            <FormLabel>{t('authorSelect.bio').toString()}</FormLabel>
                            <LocalizedInput
                                as={Textarea}
                                minLength={0}
                                maxLength={200}
                                name="bio"
                                placeholder={t('authorSelect.bio').toString()}
                                value={{ cs: '', en: '', ...author.bio }}
                                onChange={(value) => {
                                    onChange(author.set('bio', value));
                                    forceUpdate();
                                }}
                                split
                            />
                        </FormControl>
                    </>
                ) : (
                    <VStack align="stretch" spacing={8}>
                        <Box>
                            <FormLabel fontWeight="bold">{t('authorSelect.name').toString()}</FormLabel>
                            <Text>{author.name}</Text>
                        </Box>
                        <Box>
                            <FormLabel fontWeight="bold">{t('authorSelect.uco').toString()}</FormLabel>
                            {author.uco ? (
                                <Text>{author.uco}</Text>
                            ) : (
                                <Text fontStyle="italic">{t('authorSelect.missingUco').toString()}</Text>
                            )}
                        </Box>
                        <Box>
                            <FormLabel fontWeight="bold">{t('authorSelect.bio').toString()}</FormLabel>
                            {author.getBio().trim() ? (
                                <Text whiteSpace="pre-wrap">{author.getBio()}</Text>
                            ) : (
                                <Text fontStyle="italic">{t('authorSelect.missingBio').toString()}</Text>
                            )}
                        </Box>
                    </VStack>
                )}
            </ModalBody>
            <ModalFooter>
                {canEdit && (
                    <SendAPI
                        value={author}
                        request={(api, value) => isExisting ? api.authors.update(value) : api.authors.create(value)}
                        onSubmited={(id) => onSaved(author, id)}
                    >
                        {(onSubmit, sendStatus) => (
                            <Button
                                colorScheme="blue"
                                mr={3}
                                onClick={onSubmit}
                                isDisabled={sendStatus === 'sending' || sendStatus === 'ok'}
                                isLoading={sendStatus === 'sending'}
                            >
                                {isExisting ? t('generic.save').toString() : t('authorSelect.addButton').toString()}
                            </Button>
                        )}
                    </SendAPI>
                )}
                <Button onClick={onClose}>{t('generic.cancel').toString()}</Button>
            </ModalFooter>
        </ModalContent>
    );
}

interface IAuthorFieldProps {
    author: Author;
    field: 'name' | 'uco' | 'email' | 'phone';
    onChange: (author: Author) => void;
}

function AuthorField({ author, field, onChange }: IAuthorFieldProps) {
    const forceUpdate = useForceUpdate();

    return (
        <FormControl mb={6}>
            <FormLabel>{t(`authorSelect.${field}`).toString()}</FormLabel>
            <Input
                placeholder={t(`authorSelect.${field}`).toString()}
                value={author[field]}
                onChange={(event) => {
                    onChange(author.set(field, event.target.value));
                    forceUpdate();
                }}
            />
        </FormControl>
    );
}

interface IAuthorRoleModalProps {
    roles: string[];
    isDropdownCrewSelect?: boolean;
    onSaved: (roles: string[]) => void;
    onClose: () => void;
}

function AuthorRoleModal({ roles: initialRoles, isDropdownCrewSelect, onSaved, onClose }: IAuthorRoleModalProps) {
    const [roles, setRoles] = useState(initialRoles);
    const [roleString, setRoleString] = useState('');
    const { t } = useTranslation();

    return (
        <ModalContent>
            <ModalHeader>{t('authorSelect.selectRoles').toString()}</ModalHeader>
            <ModalCloseButton />
            <ModalBody pb={6}>
                <FormControl mt={6}>
                    <FormLabel>{t('authorSelect.selectRoles').toString()}</FormLabel>
                    {isDropdownCrewSelect ? (
                        <MultiSelect
                            options={crewRolesManager.getOptions()}
                            value={crewRolesManager.getValue(roles)}
                            onChange={(value) => crewRolesManager.onChange(value, setRoles)}
                        />
                    ) : (
                        <TagInput
                            placeholder={t('authorSelect.addRole').toString()}
                            tags={roles}
                            setTags={setRoles}
                            value={roleString}
                            setValue={setRoleString}
                        />
                    )}
                </FormControl>
            </ModalBody>
            <ModalFooter>
                <Button
                    colorScheme="blue"
                    mr={3}
                    onClick={() => {
                        onSaved(roleString.length > 0 ? [...roles, roleString] : roles);
                        onClose();
                    }}
                >
                    {t('generic.save').toString()}
                </Button>
                <Button onClick={onClose}>{t('generic.cancel').toString()}</Button>
            </ModalFooter>
        </ModalContent>
    );
}
