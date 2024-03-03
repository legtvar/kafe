import {
    Box,
    Button,
    FormControl,
    FormLabel,
    HStack,
    Heading,
    Input,
    Link,
    Stack,
    Text,
    useColorModeValue,
} from '@chakra-ui/react';
import i18next, { t } from 'i18next';
import { useState } from 'react';
import { useApi } from '../../../hooks/Caffeine';
import { Loading } from '../../utils/Loading';
import { MuniIcon } from '../../utils/MuniIcon';
import { useTitle } from '../../../utils/useTitle';

export function Login() {
    const [state, setState] = useState<'ready' | 'submitting' | 'submited' | 'error'>('ready');
    const [email, setEmail] = useState<string>('');
    const api = useApi();
    useTitle(t("title.login"));

    const login = async () => {
        setState('submitting');
        try {
            await api.accounts.temporary.create(email, i18next.language);
            setState('submited');
        } catch (e) {
            setState('error');
        }
    };

    return (
        <>
            <Stack align={'center'}>
                <Heading fontSize={'4xl'}>{t('login.title').toString()}</Heading>
                <Text fontSize={'lg'} color={'gray.600'}>
                    {t('register.subtitle').toString()}
                </Text>
            </Stack>
            <Box rounded={'lg'} bg={useColorModeValue('white', 'gray.700')} boxShadow={'lg'} p={8}>
                {(state === 'ready' || state === 'error') && (
                    <Stack spacing={4}>
                        <FormControl id="email" isRequired>
                            <FormLabel>{t('register.email').toString()}</FormLabel>
                            <form onSubmit={() => login()}>
                                <Input type="email" value={email} onChange={(event) => setEmail(event.target.value)} />
                            </form>
                        </FormControl>
                        {state === 'error' && (
                            <Text as="p" color="red.500">
                                {t('login.error').toString()}
                            </Text>
                        )}
                        <Stack spacing={10} pt={2}>
                            <Button
                                loadingText="Submitting"
                                size="lg"
                                bg={'blue.400'}
                                color={'white'}
                                onClick={() => login()}
                                _hover={{
                                    bg: 'blue.500',
                                }}
                            >
                                {t('register.button').toString()}
                            </Button>
                            <Link href={api.accounts.external.loginUrl()}>
                                <Button
                                    size="lg"
                                    bg={'#0000dc'}
                                    color={'white'}
                                    _hover={{
                                        bg: 'black',
                                    }}
                                    width="100%"
                                    leftIcon={<MuniIcon fill="white" />}
                                >
                                    {t('munilogin.button').toString()}
                                </Button>
                            </Link>
                        </Stack>
                    </Stack>
                )}
                {state === 'submitting' && (
                    <>
                        <Loading center large />
                    </>
                )}
                {state === 'submited' && (
                    <>
                        <Text as="p" pb={4}>
                            {t('temp.after').toString()}
                        </Text>
                        <Text as="p" pb={4}>
                            {t('temp.after2').toString()}
                        </Text>
                        <Button
                            loadingText="Send again"
                            size="lg"
                            bg={'blue.400'}
                            color={'white'}
                            onClick={() => setState('ready')}
                            _hover={{
                                bg: 'blue.500',
                            }}
                        >
                            {t('temp.back').toString()}
                        </Button>
                    </>
                )}
            </Box>
        </>
    );
}
