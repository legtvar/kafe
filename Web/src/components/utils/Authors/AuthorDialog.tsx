import {
    Box,
    Button,
    FormLabel,
    Modal,
    ModalBody,
    ModalCloseButton,
    ModalContent,
    ModalFooter,
    ModalHeader,
    ModalOverlay,
    Text,
    VStack,
} from '@chakra-ui/react';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Author } from '../../../data/Author';
import { SendAPI } from '../SendAPI';
import { AuthorInfoForm } from './AuthorInfoForm';

interface IAuthorDialogProps {
    author: Author;
    canEdit: boolean;
    onClose: () => void;
    onSaved: (author: Author) => void;
}

export function AuthorDialog({ author, canEdit, onClose, onSaved }: IAuthorDialogProps) {
    const [draft, setDraft] = useState(() => new Author(author));
    const { t } = useTranslation();

    return (
        <Modal isOpen onClose={onClose}>
            <ModalOverlay />
            <ModalContent>
                <ModalHeader>{author.name}</ModalHeader>
                <ModalCloseButton />
                <ModalBody pb={6}>
                    {canEdit ? (
                        <AuthorInfoForm author={draft} onChange={setDraft} />
                    ) : (
                        <VStack align="stretch" spacing={6}>
                            <Box>
                                <FormLabel fontWeight="bold">{t('authorSelect.name').toString()}</FormLabel>
                                <Text>{author.name}</Text>
                            </Box>
                            <Box>
                                <FormLabel fontWeight="bold">{t('authorSelect.uco').toString()}</FormLabel>
                                <Text>{author.uco}</Text>
                            </Box>
                            <Box>
                                <FormLabel fontWeight="bold">{t('authorSelect.bio').toString()}</FormLabel>
                                <Text whiteSpace="pre-wrap">{author.getBio()}</Text>
                            </Box>
                        </VStack>
                    )}
                </ModalBody>
                <ModalFooter>
                    {canEdit && (
                        <SendAPI
                            value={draft}
                            request={(api, value) => api.authors.update(value)}
                            onSubmited={() => {
                                onSaved(draft);
                                onClose();
                            }}
                        >
                            {(onSubmit, sendStatus) => (
                                <Button
                                    colorScheme="blue"
                                    mr={3}
                                    onClick={onSubmit}
                                    isDisabled={sendStatus === 'sending' || sendStatus === 'ok'}
                                    isLoading={sendStatus === 'sending'}
                                >
                                    {t('generic.save').toString()}
                                </Button>
                            )}
                        </SendAPI>
                    )}
                    <Button onClick={onClose}>{t('generic.cancel').toString()}</Button>
                </ModalFooter>
            </ModalContent>
        </Modal>
    );
}
