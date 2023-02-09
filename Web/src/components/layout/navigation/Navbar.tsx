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
import { FiBell, FiChevronDown, FiMenu, FiMoon, FiSun } from 'react-icons/fi';
import { Logo } from '../../Logo';

interface INavbarProps extends FlexProps {
    onOpen: () => void;
}
export function Navbar({ onOpen, ...rest }: INavbarProps) {
    const { colorMode, toggleColorMode } = useColorMode();

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
                                    <Avatar
                                        size={'sm'}
                                        src={
                                            'https://images.unsplash.com/photo-1619946794135-5bc917a27793?ixlib=rb-0.3.5&q=80&fm=jpg&crop=faces&fit=crop&h=200&w=200&s=b616b2c5b373a80ffc9636ba24f7a4a9'
                                        }
                                    />
                                    <VStack
                                        display={{ base: 'none', md: 'flex' }}
                                        alignItems="flex-start"
                                        spacing="1px"
                                        ml="2"
                                        w={150}
                                    >
                                        <Text fontSize="sm">Justina Clark</Text>
                                        <Text fontSize="xs" color="gray.600">
                                            Admin
                                        </Text>
                                    </VStack>
                                    <Box display={{ base: 'none', md: 'flex' }}>
                                        <FiChevronDown />
                                    </Box>
                                </HStack>
                            </MenuButton>
                        </Button>
                        <MenuList>
                            <MenuItem>Profile</MenuItem>
                            <MenuItem>Settings</MenuItem>
                            <MenuItem>Billing</MenuItem>
                            <MenuDivider />
                            <MenuItem>Sign out</MenuItem>
                        </MenuList>
                    </Menu>
                </Flex>
            </HStack>
        </Flex>
    );
}
