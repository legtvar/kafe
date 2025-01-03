import { Box, Center, Portal, Spinner } from '@chakra-ui/react';

export function PortalLoading() {
    // console.log('hello');
    return (
        <Portal>
            <Box
                position="fixed"
                inset={0}
                background="rgba(0,0,0,0.8)"
                backdropFilter="blur(10px)"
                zIndex="overlay"
            ></Box>
            <Center position="fixed" inset={0} zIndex="overlay">
                <Spinner size="xl" />
            </Center>
        </Portal>
    );
}
