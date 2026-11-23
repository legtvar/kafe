import {
    Button,
    FormControl,
    FormLabel,
    Modal,
    ModalBody,
    ModalCloseButton,
    ModalContent,
    ModalFooter,
    ModalHeader,
    ModalOverlay,
} from '@chakra-ui/react';
import { MultiSelect } from 'chakra-multiselect';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import crewRolesManager from '../../../utils/managers/crewRolesManager';
import { TagInput } from './TagInput';

interface IAuthorRolesDialogProps {
    roles: string[];
    isDropdownCrewSelect?: boolean;
    onClose: () => void;
    onSaved: (roles: string[]) => void;
}

export function AuthorRolesDialog({
    roles: initialRoles,
    isDropdownCrewSelect,
    onClose,
    onSaved,
}: IAuthorRolesDialogProps) {
    const [roles, setRoles] = useState(initialRoles);
    const [roleString, setRoleString] = useState('');
    const { t } = useTranslation();

    return (
        <Modal isOpen onClose={onClose}>
            <ModalOverlay />
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
        </Modal>
    );
}
