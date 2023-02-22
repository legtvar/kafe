import {
    Box,
    Button,
    FormControl,
    FormLabel,
    Heading,
    HStack,
    Input,
    InputGroup,
    Link,
    Stack,
    Text,
    useColorModeValue,
} from '@chakra-ui/react';
import { t } from 'i18next';
import { Trans } from 'react-i18next';
import { useNavigate } from 'react-router-dom';

export function Register() {
    const navigate = useNavigate();

    return (
        <>
            <Stack align={'center'}>
                <Heading fontSize={'4xl'}>{t('register.title').toString()}</Heading>
                <Text fontSize={'lg'} color={'gray.600'}>
                    {t('register.subtitle').toString()}
                </Text>
            </Stack>
            <Box rounded={'lg'} bg={useColorModeValue('white', 'gray.700')} boxShadow={'lg'} p={8}>
                <Stack spacing={4}>
                    <HStack>
                        <Box>
                            <FormControl id="firstName" isRequired>
                                <FormLabel>{t('register.firstName').toString()}</FormLabel>
                                <Input type="text" />
                            </FormControl>
                        </Box>
                        <Box>
                            <FormControl id="lastName" isRequired>
                                <FormLabel>{t('register.lastName').toString()}</FormLabel>
                                <Input type="text" />
                            </FormControl>
                        </Box>
                    </HStack>
                    <FormControl id="email" isRequired>
                        <FormLabel>{t('register.email').toString()}</FormLabel>
                        <Input type="email" />
                    </FormControl>
                    <FormControl id="password" isRequired>
                        <FormLabel>{t('register.password').toString()}</FormLabel>
                        <InputGroup>
                            <Input type="password" />
                        </InputGroup>
                    </FormControl>
                    <FormControl id="passwordAgain" isRequired>
                        <FormLabel>{t('register.passwordAgain').toString()}</FormLabel>
                        <InputGroup>
                            <Input type="passwordAgain" />
                        </InputGroup>
                    </FormControl>
                    <Stack spacing={10} pt={2}>
                        <Button
                            loadingText="Submitting"
                            size="lg"
                            bg={'blue.400'}
                            color={'white'}
                            _hover={{
                                bg: 'blue.500',
                            }}
                        >
                            {t('register.button').toString()}
                        </Button>
                    </Stack>
                    <Stack pt={6}>
                        <Text align={'center'}>
                            <Trans i18nKey="register.login">
                                Already a user?
                                <Link color={'blue.400'} onClick={() => navigate('/account/login')}>
                                    Login
                                </Link>
                            </Trans>
                        </Text>
                    </Stack>
                </Stack>
            </Box>
        </>
    );
}
