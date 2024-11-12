import { Button, Heading, HStack, Text, ToastId, useToast, VStack } from '@chakra-ui/react';
import { t } from 'i18next';
import { useEffect, useRef } from 'react';
import { Trans, useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';

const LS_COOKIE_CONSENT = 'kafe_cookie_consent';

export function useCookieConsent() {
    const toast = useToast();
    const toastRef = useRef<ToastId>();
    const { i18n } = useTranslation();
    const navigate = useNavigate();

    // Show the cookie consent toast once, only if the user has not already accepted it
    useEffect(() => {
        if (!localStorage.getItem(LS_COOKIE_CONSENT)) {
            toastRef.current = toast({});
            // localStorage.setItem(LS_COOKIE_CONSENT, 'true');
        }
    }, [toast]);

    useEffect(() => {
        if (!toastRef.current) return;

        toast.update(toastRef.current, {
            title: (
                <Heading as="h2" fontSize="xl" mb={4}>
                    {t('cookies.title')}
                </Heading>
            ),
            description: (
                <VStack alignItems="start">
                    <Text>
                        <VStack alignItems="start" mb={4}>
                            <Trans i18nKey="cookies.description">
                                <Text>Line1</Text>
                                <Text>Line2</Text>
                                <Text>Line3</Text>
                            </Trans>
                        </VStack>
                    </Text>
                    <HStack w="full" justify="end">
                        <Button
                            variant="outline"
                            onClick={() => {
                                window.location.replace('https://muni.cz');
                            }}
                        >
                            {t('cookies.reject')}
                        </Button>
                        <Button
                            colorScheme="blue"
                            onClick={() => {
                                localStorage.setItem(LS_COOKIE_CONSENT, 'true');
                                toast.close(toastRef.current!);
                            }}
                        >
                            {t('cookies.accept')}
                        </Button>
                    </HStack>
                </VStack>
            ),
            status: 'warning',
            duration: null,
            isClosable: false,
            variant: 'left-accent',
        });
    }, [i18n.language]);
}
