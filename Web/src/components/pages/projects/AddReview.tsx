import {
    AlertDialog,
    AlertDialogBody,
    AlertDialogContent,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogOverlay,
    Button,
    FormControl,
    HStack,
    Select,
    Stack,
    Textarea,
    useDisclosure,
} from '@chakra-ui/react';
import { t } from 'i18next';
import { createRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Project } from '../../../data/Project';
import { useAuth } from '../../../hooks/Caffeine';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { components } from '../../../schemas/api';
import { SendAPI } from '../../utils/SendAPI';

interface IAddReviewProps {
    project: Project;
}

export function AddReview(props: IAddReviewProps) {
    const { border, bg } = useColorScheme();
    const [emailContent, setEmailContent] = useState('');
    const [role, setRole] = useState<'tech' | 'visual' | 'dramaturgy' | null>(null);
    const [kind, setKind] = useState<components['schemas']['ReviewKind']>('Accepted');
    const { user } = useAuth();
    const navigate = useNavigate();
    const { isOpen, onOpen, onClose } = useDisclosure();
    const cancelRef = createRef<HTMLButtonElement>();

    console.log(emailContent, kind, role);

    const roles = user!.capabilities.includes('Administration')
        ? ['tech', 'visual', 'dramaturgy']
        : user!.capabilities.filter((cap) => cap.startsWith('ProjectReview')).map((cap) => cap.split(':')[1]);

    return (
        <SendAPI
            onSubmited={() => navigate(0)}
            request={(api) => api.review.create(props.project.id, kind, role!, emailContent)}
            value={null}
        >
            {(onSubmit, status) => (
                <Stack spacing={4} direction="column" mb={8}>
                    <FormControl>
                        <Select
                            value={role || 'Unknown'}
                            onChange={(event) => setRole(event.target.value as any)}
                            borderColor={border}
                            bg={bg}
                        >
                            <option value={'Unknown'}>{t(`project.admin.role.placeholder`).toString()}</option>
                            {roles.map((role) => (
                                <option value={role}>{t(`project.admin.role.${role}`).toString()}</option>
                            ))}
                        </Select>
                    </FormControl>
                    <FormControl>
                        <Select
                            value={kind || undefined}
                            onChange={(event) => setKind(event.target.value as any)}
                            borderColor={border}
                            bg={bg}
                        >
                            <option value={'Accepted'}>{t(`project.admin.kind.accepted`).toString()}</option>
                            <option value={'Rejected'}>{t(`project.admin.kind.rejected`).toString()}</option>
                        </Select>
                    </FormControl>
                    <FormControl>
                        <Textarea
                            borderColor={border}
                            bg={bg}
                            placeholder={t('project.admin.emailContent').toString()}
                            value={emailContent}
                            h={64}
                            onChange={(event) => setEmailContent(event.target.value)}
                        />
                    </FormControl>

                    <HStack w="100%">
                        <Button
                            colorScheme="blue"
                            ml="auto"
                            onClick={onOpen}
                            isLoading={status === 'sending'}
                            isDisabled={status === 'sending' || status === 'ok'}
                        >
                            {t('project.admin.submit').toString()}
                        </Button>

                        <AlertDialog isOpen={isOpen} leastDestructiveRef={cancelRef} onClose={onClose}>
                            <AlertDialogOverlay>
                                <AlertDialogContent>
                                    <AlertDialogHeader fontSize="lg" fontWeight="bold">
                                        {t('project.admin.submit').toString()}
                                    </AlertDialogHeader>

                                    <AlertDialogBody>{t('project.admin.confirm').toString()}</AlertDialogBody>

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
                                            {t('project.admin.submit').toString()}
                                        </Button>
                                    </AlertDialogFooter>
                                </AlertDialogContent>
                            </AlertDialogOverlay>
                        </AlertDialog>
                    </HStack>
                </Stack>
            )}
        </SendAPI>
    );
}
