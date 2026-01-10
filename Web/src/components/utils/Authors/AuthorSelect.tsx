import {
    Box,
    Button,
    FormControl,
    FormLabel,
    Heading,
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
    useDisclosure,
    VStack,
} from '@chakra-ui/react';
import { t } from 'i18next';
import { useCallback, useRef, useState } from 'react';
import { AiOutlinePlus, AiOutlineSearch } from 'react-icons/ai';
import { Author } from '../../../data/Author';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { HRIB } from '../../../schemas/generic';
import { AwaitAPI } from '../AwaitAPI';
import { SendAPI } from '../SendAPI';
import { TextareaLimited } from '../TextareaLimited';
import { TagInput } from './TagInput';
import { MultiSelect } from 'chakra-multiselect';
import crewRolesManager from '../../../utils/managers/crewRolesManager';

interface IAuthorSelectProps {
    onSelect: (id: HRIB, roles: string[]) => void;
    isDropdownCrewSelect?: boolean;
}

export function AuthorSelect(props: IAuthorSelectProps) {
    const { isOpen, onOpen, onClose } = useDisclosure();
    const [name, setName] = useState('');
    const [key, setKey] = useState(Math.random());
    const [author, setAuthor] = useState<Author | null>(null);
    const [roles, setRoles] = useState<string[]>([]);
    const [roleString, setRoleString] = useState<string>('');
    const [status, setStatus] = useState<'query' | 'new' | 'role'>('query');
    const { bg, lighten } = useColorScheme();

    const initialRef = useRef(null);

    const filter = (author: Author) => {
        const personName = author.name.toLowerCase();
        const lcQuery = name.toLowerCase();

        return personName.includes(lcQuery);
    };

    const close = () => {
        setName('');
        setAuthor(null);
        setStatus('query');
        setKey(Math.random());
        onClose();
    };

    return (
        <AwaitAPI request={useCallback((api) => api.authors.getAll(), [])}>
            {(data: Author[]) => (
                <>
                    <Button onClick={onOpen} mt={4}>
                        {t('authorSelect.button').toString()}
                    </Button>

                    <SendAPI
                        onSubmited={(id) => {
                            author!.id = id;
                            setStatus('role');
                        }}
                        value={author!}
                        request={(api, value) => api.authors.create(value)}
                        key={key}
                    >
                        {(onSubmit, sendStatus) => (
                            <Modal initialFocusRef={initialRef} isOpen={isOpen} onClose={close}>
                                <ModalOverlay />
                                <ModalContent>
                                    <ModalHeader>{t('authorSelect.button').toString()}</ModalHeader>
                                    <ModalCloseButton />
                                    <ModalBody pb={6}>
                                        {status === 'query' && (
                                            <>
                                                <FormControl>
                                                    <InputGroup>
                                                        <InputLeftElement>
                                                            <AiOutlineSearch />
                                                        </InputLeftElement>
                                                        <Input
                                                            ref={initialRef}
                                                            value={name}
                                                            onChange={(event) => setName(event.target.value)}
                                                            placeholder={t('authorSelect.name').toString()}
                                                        />
                                                    </InputGroup>
                                                </FormControl>
                                                {name.length > 0 && (
                                                    <VStack alignItems="stretch" spacing={0} mx={-6} mt={4}>
                                                        {data
                                                            .filter(filter)
                                                            .sort((a, b) => a.name.localeCompare(b.name))
                                                            .slice(0, 5)
                                                            .map((author, key) => (
                                                                <Box
                                                                    px={8}
                                                                    py={4}
                                                                    cursor="pointer"
                                                                    _hover={{ background: bg }}
                                                                    key={key}
                                                                    onClick={() => {
                                                                        setAuthor(author);
                                                                        setStatus('role');
                                                                    }}
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
                                                            onClick={() => {
                                                                setAuthor(
                                                                    new Author({
                                                                        name,
                                                                        visibility: 'Private',
                                                                    } as any),
                                                                );
                                                                setStatus('new');
                                                            }}
                                                        >
                                                            <AiOutlinePlus />{' '}
                                                            <Text>{t('authorSelect.new').toString()}</Text>
                                                        </HStack>
                                                    </VStack>
                                                )}
                                            </>
                                        )}

                                        {status === 'new' && (
                                            <>
                                                <FormControl mb={6}>
                                                    <FormLabel>{t('authorSelect.name').toString()}</FormLabel>
                                                    <Input
                                                        placeholder={t('authorSelect.name').toString()}
                                                        defaultValue={author!.name}
                                                        onChange={(event) =>
                                                            setAuthor(author!.set('name', event.target.value))
                                                        }
                                                    />
                                                </FormControl>

                                                <FormControl mb={6}>
                                                    <FormLabel>{t('authorSelect.uco').toString()}</FormLabel>
                                                    <Input
                                                        placeholder={t('authorSelect.uco').toString()}
                                                        defaultValue={author!.uco}
                                                        onChange={(event) =>
                                                            setAuthor(author!.set('uco', event.target.value))
                                                        }
                                                    />
                                                </FormControl>

                                                <FormControl mb={6}>
                                                    <FormLabel>{t('authorSelect.email').toString()}</FormLabel>
                                                    <Input
                                                        placeholder={t('authorSelect.email').toString()}
                                                        defaultValue={author!.uco}
                                                        onChange={(event) =>
                                                            setAuthor(author!.set('email', event.target.value))
                                                        }
                                                    />
                                                </FormControl>

                                                <FormControl mb={6}>
                                                    <FormLabel>{t('authorSelect.phone').toString()}</FormLabel>
                                                    <Input
                                                        placeholder={t('authorSelect.phone').toString()}
                                                        defaultValue={author!.uco}
                                                        onChange={(event) =>
                                                            setAuthor(author!.set('phone', event.target.value))
                                                        }
                                                    />
                                                </FormControl>

                                                <FormControl>
                                                    <FormLabel>{t('authorSelect.bio').toString()}</FormLabel>
                                                    <TextareaLimited
                                                        placeholder={`${t('authorSelect.bio').toString()} ${t(
                                                            'createProject.language.cs',
                                                        )}`}
                                                        max={200}
                                                        defaultValue={author!.bio?.cs}
                                                        onChange={(event) =>
                                                            setAuthor(
                                                                author!.set('bio', {
                                                                    ...author!.bio,
                                                                    cs: event.target.value,
                                                                }),
                                                            )
                                                        }
                                                        mb={4}
                                                    />
                                                    <TextareaLimited
                                                        placeholder={`${t('authorSelect.bio').toString()} ${t(
                                                            'createProject.language.en',
                                                        )}`}
                                                        max={200}
                                                        defaultValue={author!.bio?.en}
                                                        onChange={(event) =>
                                                            setAuthor(
                                                                author!.set('bio', {
                                                                    ...author!.bio,
                                                                    en: event.target.value,
                                                                }),
                                                            )
                                                        }
                                                    />
                                                </FormControl>
                                            </>
                                        )}

                                        {status === 'role' && (
                                            <>
                                                <Heading as="h4" fontSize="md">
                                                    {author!.name}
                                                </Heading>
                                                <FormControl mt={6}>
                                                    <FormLabel>{t('authorSelect.selectRoles').toString()}</FormLabel>
                                                    {props.isDropdownCrewSelect ?
                                                    <MultiSelect
                                                        options={crewRolesManager.getOptions()}
                                                        value={crewRolesManager.getValue(roles)}
                                                        onChange={value => crewRolesManager.onChange(value, setRoles)}
                                                    /> :
                                                    <TagInput
                                                        placeholder={t('authorSelect.addRole').toString()}
                                                        tags={roles}
                                                        setTags={(tags) => setRoles(tags)}
                                                        value={roleString}
                                                        setValue={(str) => setRoleString(str)}
                                                    />}
                                                </FormControl>
                                            </>
                                        )}
                                    </ModalBody>

                                    <ModalFooter>
                                        {status !== 'new' && (
                                            <Button
                                                colorScheme="blue"
                                                mr={3}
                                                isDisabled={status !== 'role'}
                                                onClick={() => {
                                                    if (roleString.length > 0) {
                                                        props.onSelect(author!.id, [...roles, roleString]);
                                                    } else {
                                                        props.onSelect(author!.id, roles);
                                                    }
                                                    close();
                                                }}
                                            >
                                                {t('authorSelect.addButton').toString()}
                                            </Button>
                                        )}
                                        {status === 'new' && (
                                            <Button
                                                colorScheme="blue"
                                                mr={3}
                                                onClick={() => onSubmit()}
                                                isDisabled={sendStatus === 'sending' || sendStatus === 'ok'}
                                                isLoading={sendStatus === 'sending'}
                                            >
                                                {t('authorSelect.addButton').toString()}
                                            </Button>
                                        )}
                                        <Button onClick={close}>{t('generic.cancel').toString()}</Button>
                                    </ModalFooter>
                                </ModalContent>
                            </Modal>
                        )}
                    </SendAPI>
                </>
            )}
        </AwaitAPI>
    );
}
