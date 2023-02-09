import { Box, Drawer, DrawerContent, useDisclosure } from '@chakra-ui/react';
import React from 'react';
import { Outlet } from 'react-router-dom';
import { Navbar } from './navigation/Navbar';
import { Sidebar } from './navigation/Sidebar';

export const Root: React.FC = () => {
    const { isOpen, onOpen, onClose } = useDisclosure();

    return (
        <Box minH="100vh">
            <Sidebar onClose={() => onClose} display={{ base: 'none', md: 'block' }} />
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
                    <Sidebar onClose={onClose} />
                </DrawerContent>
            </Drawer>
            <Navbar onOpen={onOpen} />
            <Box ml={{ base: 0, md: 64 }} p="4" height="calc(100vh - 80px)">
                <Outlet />
            </Box>
        </Box>
    );
};
