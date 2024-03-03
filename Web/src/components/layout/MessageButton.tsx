import {
    Box,
    BoxProps,
    Button,
    IconButton,
    Link,
    Modal,
    ModalBody,
    ModalCloseButton,
    ModalContent,
    ModalFooter,
    ModalHeader,
    ModalOverlay,
    Text,
    useDisclosure,
} from '@chakra-ui/react';
import { t } from 'i18next';
import { Trans } from 'react-i18next';
import { AiFillWarning } from 'react-icons/ai';
import { Autolink } from '../utils/Autolink';
import { Autoemail } from '../utils/Autoemail';

interface IMessageButtonProps extends BoxProps {
    titleKey: string;
    warningKey: string;
    descriptionKey: string;
}

export function MessageButton({ titleKey, warningKey, descriptionKey, ...box }: IMessageButtonProps) {
    const { isOpen, onOpen, onClose } = useDisclosure();

    return (
        <Box {...box}>
            <IconButton
                display={{ base: 'flex', lg: 'none' }}
                size="lg"
                variant="ghost"
                aria-label="Warning"
                color="yellow.500"
                icon={<AiFillWarning />}
                onClick={onOpen}
                width="100%"
            />
            <Button
                display={{ base: 'none', lg: 'flex' }}
                size="lg"
                variant="ghost"
                aria-label="Warning"
                color="yellow.500"
                leftIcon={<AiFillWarning />}
                onClick={onOpen}
                width="100%"
            >
                {t(warningKey).toString()}
            </Button>

            <Modal isOpen={isOpen} onClose={onClose}>
                <ModalOverlay />
                <ModalContent>
                    <ModalHeader>{t(titleKey).toString()}</ModalHeader>
                    <ModalCloseButton />
                    <ModalBody>
                        <Text mb={4}>
                            <Trans i18nKey={descriptionKey} components={{ link: <Autolink />, email: <Autoemail /> }} />
                        </Text>
                    </ModalBody>

                    <ModalFooter>
                        <Button colorScheme="blue" mr={3} onClick={onClose}>
                            {t('common.understood').toString()}
                        </Button>
                    </ModalFooter>
                </ModalContent>
            </Modal>
        </Box>
    );
}
