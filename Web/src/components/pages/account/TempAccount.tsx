import { Box, Button, FormControl, FormLabel, Heading, Input, Stack, Text, useColorModeValue } from '@chakra-ui/react';
import i18next, { t } from 'i18next';
import { useState } from 'react';
import { useApi } from '../../../hooks/Caffeine';
import { Loading } from '../../utils/Loading';

export function TempAccount() {
    const [state, setState] = useState<'ready' | 'submitting' | 'submited' | 'error'>('ready');
    const [email, setEmail] = useState<string>('');
    const api = useApi();

    return (
        <>
            <Stack align={'center'}>
                <Heading fontSize={'4xl'}>{t('tempAccount.title').toString()}</Heading>
                <Text fontSize={'lg'} color={'gray.600'}>
                    {t('register.subtitle').toString()}
                </Text>
            </Stack>
            <Box rounded={'lg'} bg={useColorModeValue('white', 'gray.700')} boxShadow={'lg'} p={8}>
                {state === 'ready' && (
                    <Stack spacing={4}>
                        <FormControl id="email" isRequired>
                            <FormLabel>{t('register.email').toString()}</FormLabel>
                            <Input type="email" value={email} onChange={(event) => setEmail(event.target.value)} />
                        </FormControl>
                        <Stack spacing={10} pt={2}>
                            <Button
                                loadingText="Submitting"
                                size="lg"
                                bg={'blue.400'}
                                color={'white'}
                                onClick={async () => {
                                    setState('submitting');
                                    const response = await api.accounts.temporary.create(email, i18next.language);
                                    setState('submited');
                                }}
                                _hover={{
                                    bg: 'blue.500',
                                }}
                            >
                                {t('register.button').toString()}
                            </Button>
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
