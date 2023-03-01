import {
    Box,
    Button,
    Checkbox,
    FormControl,
    FormLabel,
    Heading,
    Input,
    Link,
    Stack,
    Text,
    useColorModeValue,
} from '@chakra-ui/react';
import { t } from 'i18next';
import { Trans } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { User } from '../../../data/User';
import { useAuth } from '../../../hooks/Caffeine';

export function Login() {
    const navigate = useNavigate();
    const { setUser } = useAuth();

    return (
        <>
            <Stack align={'center'}>
                <Heading fontSize={'4xl'}>{t('login.title').toString()}</Heading>
                <Text fontSize={'lg'} color={'gray.600'}>
                    {t('login.subtitle').toString()}
                </Text>
            </Stack>
            <Box rounded={'lg'} bg={useColorModeValue('white', 'gray.700')} boxShadow={'lg'} p={8}>
                <Stack spacing={4}>
                    <FormControl id="email">
                        <FormLabel>{t('login.email').toString()}</FormLabel>
                        <Input type="email" />
                    </FormControl>
                    <FormControl id="password">
                        <FormLabel>{t('login.password').toString()}</FormLabel>
                        <Input type="password" />
                    </FormControl>
                    <Stack spacing={10} pt={2}>
                        <Stack direction={{ base: 'column', sm: 'row' }} align={'start'} justify={'space-between'}>
                            <Checkbox>{t('login.remember').toString()}</Checkbox>
                            <Link color={'blue.400'} onClick={() => navigate('/account/forgot')}>
                                {t('login.forgot').toString()}
                            </Link>
                        </Stack>
                        <Button
                            loadingText="Submitting"
                            size="lg"
                            bg={'blue.400'}
                            color={'white'}
                            onClick={() => {
                                const user = new User();
                                user.email = 'rosecky.jonas@gmail.com';
                                user.id = '123456789';
                                user.name = 'Jonáš Rosecký';
                                user.role = 'admin';

                                setUser(user);

                                navigate('/auth');
                            }}
                            _hover={{
                                bg: 'blue.500',
                            }}
                        >
                            {t('login.button').toString()}
                        </Button>
                    </Stack>
                    <Stack pt={6}>
                        <Text align={'center'}>
                            <Trans i18nKey="login.register">
                                Don't have an account?
                                <Link color={'blue.400'} onClick={() => navigate('/account/register')}>
                                    Register now!
                                </Link>
                            </Trans>
                        </Text>
                    </Stack>
                </Stack>
            </Box>
        </>
    );
}
