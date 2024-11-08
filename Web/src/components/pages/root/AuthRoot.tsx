import { Box, Drawer, DrawerContent, Flex, useDisclosure } from '@chakra-ui/react';
import React from 'react';
import { Outlet } from 'react-router-dom';
import { useReloadVar } from '../../../hooks/useReload';
import { Navbar } from '../../layout/navigation/Navbar';
import { Sidebar } from '../../layout/navigation/Sidebar';
import { ErrorBoundary } from '../../utils/ErrorBoundary';
export const AuthRoot: React.FC = () => {
    const { isOpen, onOpen, onClose } = useDisclosure();
    const { reload, value } = useReloadVar();

    return (
        <Flex direction="column" w="100vw" h="100vh" align="stretch">
            <Navbar onOpen={() => onOpen()} signedIn={true} forceReload={() => reload()} />
            <Flex direction="row" flexGrow={1} align="stretch" minH={0} minW={0}>
                <Sidebar onClose={() => onClose} display={{ base: 'none', md: 'flex' }} forceReload={() => reload()} />
                <Drawer
                    autoFocus={false}
                    isOpen={isOpen}
                    placement="left"
                    onClose={onClose}
                    returnFocusOnClose={false}
                    onOverlayClick={onClose}
                    size="xs"
                >
                    <DrawerContent>
                        <Sidebar onClose={onClose} forceReload={() => reload()} />
                    </DrawerContent>
                </Drawer>
                <Box px={4} pt={4} flexGrow={1} overflowY="auto">
                    <ErrorBoundary>
                        <Outlet key={value ? 'a' : 'b'} />
                    </ErrorBoundary>
                </Box>
            </Flex>
        </Flex>
    );
};
