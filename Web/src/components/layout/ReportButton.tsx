import {
    Box,
    BoxProps,
    Button,
    Checkbox,
    Code,
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
    Textarea,
    useColorModeValue,
    useDisclosure,
    VStack,
} from '@chakra-ui/react';
import { t } from 'i18next';
import { useState } from 'react';
import CopyToClipboard from 'react-copy-to-clipboard';
import { Trans } from 'react-i18next';
import { AiFillWarning } from 'react-icons/ai';
import { IoCheckmark, IoCopyOutline } from 'react-icons/io5';
import { logs } from '../../consoleOverride';
import { useAuth } from '../../hooks/Caffeine';

interface IReportButtonProps extends BoxProps {}

export function ReportButton({ ...box }: IReportButtonProps) {
    const { isOpen, onOpen, onClose } = useDisclosure();
    const [copied, setCopied] = useState(false);
    const [attachedItems, setAttachedItems] = useState<{
        page: boolean;
        browser: boolean;
        user: boolean;
        logs: boolean;
    }>({
        page: true,
        browser: true,
        user: true,
        logs: true,
    });
    const [message, setMessage] = useState('');

    const report = {
        date: new Date(),
        page: attachedItems.page ? window.location.href : undefined,
        browser: attachedItems.browser ? navigator.userAgent : undefined,
        screenSize: attachedItems.browser ? `${window.screen.width}x${window.screen.height}` : undefined,
        user: attachedItems.user ? useAuth().user : undefined,
        logs: attachedItems.logs ? logs : undefined,
    };

    const mailContents = t('troubleshooting.mailContents', { message }) + JSON.stringify(report, undefined, 4);

    const closeHandler = () => {
        onClose();
        setCopied(false);
    };

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
                variant="ghost"
                aria-label="Warning"
                color="yellow.500"
                leftIcon={<AiFillWarning />}
                onClick={onOpen}
                width="100%"
                justifyContent="start"
            >
                <Text ml={2}>{t('troubleshooting.title').toString()}</Text>
            </Button>

            <Modal isOpen={isOpen} onClose={closeHandler} size="xl">
                <ModalOverlay />
                <ModalContent>
                    <ModalHeader>{t('troubleshooting.title').toString()}</ModalHeader>
                    <ModalCloseButton />
                    <ModalBody>
                        <Text mb={8}>{t('troubleshooting.contactUs')}</Text>
                        <Text mb={2}>{t('troubleshooting.selectAttachments')}</Text>
                        <VStack align="start" mb={8}>
                            <Checkbox
                                isChecked={attachedItems.page}
                                onChange={() => setAttachedItems({ ...attachedItems, page: !attachedItems.page })}
                            >
                                {t('troubleshooting.page')}
                            </Checkbox>
                            <Checkbox
                                isChecked={attachedItems.browser}
                                onChange={() => setAttachedItems({ ...attachedItems, browser: !attachedItems.browser })}
                            >
                                {t('troubleshooting.browser')}
                            </Checkbox>
                            <Checkbox
                                isChecked={attachedItems.user}
                                onChange={() => setAttachedItems({ ...attachedItems, user: !attachedItems.user })}
                            >
                                {t('troubleshooting.user')}
                            </Checkbox>
                            <Checkbox
                                isChecked={attachedItems.logs}
                                onChange={() => setAttachedItems({ ...attachedItems, logs: !attachedItems.logs })}
                            >
                                {t('troubleshooting.logs')}
                            </Checkbox>
                        </VStack>
                        <Text mb={2}>{t('troubleshooting.description')}</Text>
                        <Textarea value={message} onChange={(e) => setMessage(e.target.value)} mb={8} />
                        <Text mb={2}>{t('troubleshooting.generated')}</Text>
                        <Code
                            fontSize="sm"
                            p={4}
                            borderRadius="md"
                            bg={useColorModeValue('gray.100', 'gray.800')}
                            maxH="64"
                            maxW="full"
                            overflow="auto"
                            whiteSpace="pre-wrap"
                            position="relative"
                        >
                            {mailContents}
                            <CopyToClipboard
                                text={mailContents}
                                onCopy={() => {
                                    setCopied(true);
                                }}
                            >
                                <IconButton
                                    position="absolute"
                                    top={2}
                                    right={2}
                                    aria-label="copy"
                                    icon={copied ? <IoCheckmark /> : <IoCopyOutline />}
                                />
                            </CopyToClipboard>
                        </Code>
                        <Text mb={4}>
                            <Trans i18nKey="troubleshooting.sendTo">
                                Send this report to <Link href="mailto:kafe@fi.muni.cz">kafe@fi.muni.cz</Link>. Thank
                                you!
                            </Trans>
                        </Text>
                    </ModalBody>

                    <ModalFooter>
                        {/* <Button variant="outline" colorScheme="blue" mr={3} onClick={closeHandler}>
                            {t('common.cancel').toString()}
                        </Button> */}
                        {/* <Button colorScheme="blue" mr={3} onClick={closeHandler}>
                            {t('common.send').toString()}
                        </Button> */}
                        <Button colorScheme="blue" onClick={closeHandler}>
                            {t('common.understood').toString()}
                        </Button>
                    </ModalFooter>
                </ModalContent>
            </Modal>
        </Box>
    );
}
