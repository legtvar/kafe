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
import { FiChevronDown, FiMenu } from 'react-icons/fi';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useApi, useAuth } from '../../../hooks/Caffeine';
import { ColorModeToggle } from '../../utils/ColorModeToggle';
import { KafeAvatar } from '../../utils/KafeAvatar';
import { LanguageToggle } from '../../utils/LanguageToggle';
import { Logo } from '../Logo';
import { MessageButton } from '../MessageButton';
import { useAuthLinkFunction } from '../../../hooks/useAuthLink';
import { ConfirmExitLink } from '../../utils/ConfirmExitLink';

export const NAVBAR_HEIGHT = 20;

interface INavbarProps extends FlexProps {
    onOpen?: () => void;
    forceReload: () => void;
    signedIn: boolean;
}
export function Navbar({ onOpen, forceReload, signedIn, ...rest }: INavbarProps) {
    const { toggleColorMode } = useColorMode();
    const { user, setUser } = useAuth();
    const location = useLocation();
    const navigate = useNavigate();
    const authLink = useAuthLinkFunction();

    const api = useApi();

    const toggleLanguage = () => {
        const lang = i18next.language === 'en' ? 'cs' : 'en';
        i18next.changeLanguage(lang);
        forceReload();
    };

    return (
        <Flex
            px={{ base: 4, md: 4 }}
            height={NAVBAR_HEIGHT}
            alignItems="center"
            bg={useColorModeValue('white', 'gray.900')}
            borderBottomWidth="1px"
            borderBottomColor={useColorModeValue('gray.200', 'gray.700')}
            justifyContent={{ base: 'space-between', md: 'flex-end' }}
            zIndex="sticky"
            {...rest}
        >
            {signedIn && onOpen && (
                <IconButton
                    display={{ base: 'flex', md: 'none' }}
                    onClick={onOpen}
                    variant="ghost"
                    aria-label="Open menu"
                    icon={<FiMenu />}
                />
            )}

            {signedIn && <Spacer display={{ base: 'flex', md: 'none' }} />}

            <Flex h="20" alignItems="center" ml={2} mr={8} justifyContent="space-between" key="heading">
                <Link to="/">
                    <Logo />
                </Link>
            </Flex>
            
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

            {signedIn && <Spacer display={{ base: 'flex', md: 'none' }} />}

            <Flex h="20" alignItems="center" ml={2} mr={8} justifyContent="space-between" key="heading">
                <ConfirmExitLink
                    alertCondition={location.pathname.endsWith("edit") || location.pathname.endsWith("create")}
                    destPath={authLink("/")}
                    handleConfirm={() => {}}
                >
                    <Logo />
                </ConfirmExitLink>
            </Flex>

            <Spacer />

            <HStack spacing={1}>
                <LanguageToggle
                    aria-label="Language"
                    onLanguageToggled={() => forceReload()}
                    display={{ base: 'none', md: 'flex' }}
                />
                <ColorModeToggle display={{ base: 'none', md: 'flex' }} aria-label="Toggle Color Mode" />
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
                                    <HStack display={{ base: 'flex', md: 'none' }} px={2}>
                                        <LanguageToggle aria-label="Language" onLanguageToggled={() => forceReload()} />
                                        <ColorModeToggle aria-label="Toggle Color Mode" />
                                        {!signedIn && (
                                            <Link to="/account/login">
                                                <Button ml={4}>{t('home.signin').toString()}</Button>
                                            </Link>
                                        )}
                                    </HStack>
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
