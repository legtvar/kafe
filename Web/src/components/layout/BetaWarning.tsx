import {
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
import { AiFillWarning } from 'react-icons/ai';

interface IBetaWarningProps {}

export function BetaWarning(props: IBetaWarningProps) {
    const { isOpen, onOpen, onClose } = useDisclosure();

    return (
        <>
            <IconButton
                display={{ base: 'flex', lg: 'none' }}
                size="lg"
                variant="ghost"
                aria-label="Warning"
                color="yellow.500"
                icon={<AiFillWarning />}
                onClick={onOpen}
            />
            <Button
                display={{ base: 'none', lg: 'flex' }}
                size="lg"
                variant="ghost"
                aria-label="Warning"
                color="yellow.500"
                leftIcon={<AiFillWarning />}
                onClick={onOpen}
            >
                {t('beta.warning').toString()}
            </Button>

            <Modal isOpen={isOpen} onClose={onClose}>
                <ModalOverlay />
                <ModalContent>
                    <ModalHeader>{t('beta.warning').toString()}</ModalHeader>
                    <ModalCloseButton />
                    <ModalBody>
                        <Text mb={4}>{t('beta.description').toString()}</Text>
                        <Text mb={4}>
                            {t('beta.contactus').toString()} <Link href="mailto:kafe@fi.muni.cz">kafe@fi.muni.cz</Link>
                        </Text>
                        <Text></Text>
                    </ModalBody>

                    <ModalFooter>
                        <Button colorScheme="blue" mr={3} onClick={onClose}>
                            {t('beta.button').toString()}
                        </Button>
                    </ModalFooter>
                </ModalContent>
            </Modal>
        </>
    );
}
