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
    Text,
    useColorMode,
    useColorModeValue,
    VStack,
} from '@chakra-ui/react';
import { t } from 'i18next';
import { FiBell, FiChevronDown, FiMenu, FiMoon, FiSun } from 'react-icons/fi';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../hooks/Caffeine';
import { avatarUrl } from '../../../utils/avatarUrl';
import { Logo } from '../Logo';

interface INavbarProps extends FlexProps {
    onOpen: () => void;
}
export function Navbar({ onOpen, ...rest }: INavbarProps) {
    const { colorMode, toggleColorMode } = useColorMode();
    const { user } = useAuth();
    const navigate = useNavigate();

    return (
        <Flex
            ml={{ base: 0, md: 60 }}
            px={{ base: 4, md: 4 }}
            height="20"
            alignItems="center"
            bg={useColorModeValue('white', 'gray.900')}
            borderBottomWidth="1px"
            borderBottomColor={useColorModeValue('gray.200', 'gray.700')}
            justifyContent={{ base: 'space-between', md: 'flex-end' }}
            {...rest}
        >
            <IconButton
                display={{ base: 'flex', md: 'none' }}
                onClick={onOpen}
                variant="ghost"
                aria-label="Open menu"
                icon={<FiMenu />}
            />

            <Logo display={{ base: 'flex', md: 'none' }} />

            <HStack spacing={1}>
                <IconButton
                    size="lg"
                    variant="ghost"
                    aria-label="Toggle Color Mode"
                    onClick={toggleColorMode}
                    icon={colorMode === 'light' ? <FiMoon /> : <FiSun />}
                />
                <IconButton size="lg" variant="ghost" aria-label="Notifications" icon={<FiBell />} />
                <Flex alignItems={'center'}>
                    <Menu>
                        <Button size="lg" variant="ghost" px={2} ml={{ base: 0, md: 4 }}>
                            <MenuButton>
                                <HStack>
                                    <Avatar size={'sm'} src={avatarUrl(user!)} />
                                    <VStack
                                        display={{ base: 'none', md: 'flex' }}
                                        alignItems="flex-start"
                                        spacing="1px"
                                        ml="2"
                                        w={150}
                                    >
                                        <Text fontSize="sm">{user?.name}</Text>
                                        <Text fontSize="xs" color={useColorModeValue('gray.500', 'gray.500')}>
                                            {t(`role.${user?.role}`).toString()}
                                        </Text>
                                    </VStack>
                                    <Box display={{ base: 'none', md: 'flex' }}>
                                        <FiChevronDown />
                                    </Box>
                                </HStack>
                            </MenuButton>
                        </Button>
                        <MenuList>
                            <MenuItem>{t('navbar.profile').toString()}</MenuItem>
                            <MenuItem>{t('navbar.settings').toString()}</MenuItem>
                            <MenuDivider />
                            <MenuItem onClick={() => navigate('/login')}>{t('navbar.signout').toString()}</MenuItem>
                        </MenuList>
                    </Menu>
                </Flex>
            </HStack>
        </Flex>
    );
}
