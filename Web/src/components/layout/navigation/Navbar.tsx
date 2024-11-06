import {
    Box,
    Button,
    Flex,
    FlexProps,
    HStack,
    IconButton,
    Menu,
    MenuButton,
    MenuDivider,
    MenuItem,
    MenuList,
    Spacer,
    Text,
    useColorMode,
    useColorModeValue,
    VStack,
} from '@chakra-ui/react';
import i18next, { t } from 'i18next';
import { FiChevronDown, FiMenu, FiMoon, FiSun } from 'react-icons/fi';
import { Link, useNavigate } from 'react-router-dom';
import { useApi, useAuth } from '../../../hooks/Caffeine';
import { KafeAvatar } from '../../utils/KafeAvatar';
import { LanguageToggle } from '../../utils/LanguageToggle';
import { Logo } from '../Logo';
import { MessageButton } from '../MessageButton';

interface INavbarProps extends FlexProps {
    onOpen?: () => void;
    forceReload: () => void;
    signedIn: boolean;
}
export function Navbar({ onOpen, forceReload, signedIn, ...rest }: INavbarProps) {
    const { colorMode, toggleColorMode } = useColorMode();
    const { user, setUser } = useAuth();
    const navigate = useNavigate();
    const api = useApi();

    const toggleLanguage = () => {
        const lang = i18next.language === 'en' ? 'cs' : 'en';
        i18next.changeLanguage(lang);
        forceReload();
    };

    return (
        <Flex
            px={{ base: 4, md: 4 }}
            height="20"
            alignItems="center"
            bg={useColorModeValue('white', 'gray.900')}
            borderBottomWidth="1px"
            borderBottomColor={useColorModeValue('gray.200', 'gray.700')}
            justifyContent={{ base: 'space-between', md: 'flex-end' }}
            position="fixed"
            top={0}
            right={0}
            left={signedIn ? { base: 0, md: 64 } : 0}
            zIndex="sticky"
            {...rest}
        >
            {signedIn && (
                <IconButton
                    display={{ base: 'flex', md: 'none' }}
                    onClick={onOpen}
                    variant="ghost"
                    aria-label="Open menu"
                    icon={<FiMenu />}
                />
            )}

            {api.isStaging && (
                <MessageButton
                    warningKey="staging.warning"
                    titleKey="staging.title"
                    descriptionKey="staging.description"
                />
            )}
            {api.isLocalhost && (
                <MessageButton
                    warningKey="localhost.warning"
                    titleKey="localhost.title"
                    descriptionKey="localhost.description"
                />
            )}

            {signedIn && <Spacer />}
            <Link to="/">
                <Logo display={signedIn ? { base: 'flex', md: 'none' } : 'flex'} ml={6} />
            </Link>

            <Spacer />

            <HStack spacing={1}>
                <LanguageToggle
                    aria-label="Language"
                    onLanguageToggled={() => forceReload()}
                    display={{ base: 'none', md: 'flex' }}
                />
                <IconButton
                    display={{ base: 'none', md: 'flex' }}
                    size="lg"
                    variant="ghost"
                    aria-label="Toggle Color Mode"
                    onClick={toggleColorMode}
                    icon={colorMode === 'light' ? <FiMoon /> : <FiSun />}
                />
                {!signedIn && (
                    <Link to="/account/login">
                        <Button ml={4}>{t('home.signin').toString()}</Button>
                    </Link>
                )}
                {signedIn && (
                    <>
                        <Flex alignItems={'center'}>
                            <Menu>
                                <MenuButton
                                    as={Button}
                                    size="lg"
                                    variant="ghost"
                                    px={2}
                                    ml={{ base: 0, md: 4 }}
                                    fontWeight="normal"
                                >
                                    <HStack>
                                        <KafeAvatar size={'sm'} person={user!} />
                                        <VStack
                                            display={{ base: 'none', md: 'flex' }}
                                            alignItems="flex-start"
                                            spacing="1px"
                                            ml="2"
                                            w={150}
                                        >
                                            <Text maxW="100%" fontSize="sm" isTruncated>
                                                {user?.name}
                                            </Text>
                                            {/* TODO: Maybe show some role information once its implemented in the API.
                                            <Text maxW="100%" fontSize="xs" isTruncated color={'gray.500'}>
                                                {t(`role.${user?.role}`).toString()}
                                            </Text> */}
                                        </VStack>
                                        <Box display={{ base: 'none', md: 'flex' }}>
                                            <FiChevronDown />
                                        </Box>
                                    </HStack>
                                </MenuButton>
                                <MenuList>
                                    <MenuItem display={{ base: 'flex', md: 'none' }} onClick={() => toggleLanguage()}>
                                        {t('navbar.language').toString()}
                                    </MenuItem>
                                    <MenuItem display={{ base: 'flex', md: 'none' }} onClick={toggleColorMode}>
                                        {t('navbar.colorMode').toString()}
                                    </MenuItem>
                                    <MenuDivider display={{ base: 'block', md: 'none' }} />
                                    <MenuItem
                                        onClick={() => {
                                            setUser(null);
                                            api.accounts.logout();
                                            navigate('/');
                                        }}
                                    >
                                        {t('navbar.signout').toString()}
                                    </MenuItem>
                                </MenuList>
                            </Menu>
                        </Flex>
                    </>
                )}
            </HStack>
        </Flex>
    );
}
