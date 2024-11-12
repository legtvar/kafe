import {
    Box,
    Button,
    FormControl,
    FormLabel,
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
import { useTitle } from '../../../utils/useTitle';
import { Loading } from '../../utils/Loading';
import { MuniIcon } from '../../utils/MuniIcon';

export function Login() {
    const [state, setState] = useState<'ready' | 'submitting' | 'submited' | 'error'>('ready');
    const [email, setEmail] = useState<string>('');
    const api = useApi();
    useTitle(t('title.login'));

    const login = async () => {
        setState('submitting');
        try {
            const response = await api.accounts.temporary.create(email, i18next.language);
            if (response.status !== 200) {
                throw new Error('Error');
            }
            setState('submited');
        } catch (e) {
            setState('error');
        }
    };

    const MUNI_bg = useColorModeValue('#0000dc', 'white');
    const MUNI_color = useColorModeValue('white', '#0000dc');
    const MUNI_hover_bg = useColorModeValue('black', 'gray.300');

    const border = useColorModeValue('gray.200', 'gray.600');
    const internalBg = useColorModeValue('white', 'gray.700');
    const lowlight = useColorModeValue('gray.300', 'gray.500');

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
                            <Box
                                borderStyle="solid"
                                borderWidth={0}
                                borderBottomWidth={1}
                                borderColor={border}
                                position="relative"
                            >
                                <Box
                                    position="absolute"
                                    top="0"
                                    left="50%"
                                    transform="translate(-50%, -50%)"
                                    bg={internalBg}
                                    p={4}
                                    color={lowlight}
                                >
                                    {t('register.divider').toString()}
                                </Box>
                            </Box>
                            <Link href={api.accounts.external.loginUrl()}>
                                <Button
                                    size="lg"
                                    bg={MUNI_bg}
                                    color={MUNI_color}
                                    _hover={{
                                        bg: MUNI_hover_bg,
                                    }}
                                    width="100%"
                                    leftIcon={<MuniIcon fill={MUNI_color} />}
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
