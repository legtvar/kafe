import {
    Avatar,
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
import { t } from 'i18next';
import { FiBell, FiChevronDown, FiMenu, FiMoon, FiSun } from 'react-icons/fi';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../../hooks/Caffeine';
import { avatarUrl } from '../../../utils/avatarUrl';
import { LanguageToggle } from '../../utils/LanguageToggle';
import { Logo } from '../Logo';

interface INavbarProps extends FlexProps {
    onOpen?: () => void;
    forceReload: () => void;
    signedIn: boolean;
}
export function Navbar({ onOpen, forceReload, signedIn, ...rest }: INavbarProps) {
    const { colorMode, toggleColorMode } = useColorMode();
    const { user, setUser } = useAuth();
    const navigate = useNavigate();

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

            {signedIn && <Spacer />}
            <Link to="/">
                <Logo display={signedIn ? { base: 'flex', md: 'none' } : 'flex'} ml={6} />
            </Link>
            <Spacer />

            <HStack spacing={1}>
                <LanguageToggle onChange={() => forceReload()} />
                <IconButton
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
                        <IconButton size="lg" variant="ghost" aria-label="Notifications" icon={<FiBell />} />
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
                                        <Avatar size={'sm'} src={avatarUrl(user!.email)} />
                                        <VStack
                                            display={{ base: 'none', md: 'flex' }}
                                            alignItems="flex-start"
                                            spacing="1px"
                                            ml="2"
                                            w={150}
                                        >
                                            <Text fontSize="sm">{user?.name}</Text>
                                            <Text fontSize="xs" color={'gray.500'}>
                                                {t(`role.${user?.role}`).toString()}
                                            </Text>
                                        </VStack>
                                        <Box display={{ base: 'none', md: 'flex' }}>
                                            <FiChevronDown />
                                        </Box>
                                    </HStack>
                                </MenuButton>
                                <MenuList>
                                    <MenuItem>{t('navbar.profile').toString()}</MenuItem>
                                    <MenuItem>{t('navbar.settings').toString()}</MenuItem>
                                    <MenuDivider />
                                    <MenuItem
                                        onClick={() => {
                                            setUser(null);
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
