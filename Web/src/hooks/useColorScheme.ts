import { useColorModeValue } from '@chakra-ui/react';

export function useColorScheme() {
    return {
        border: useColorModeValue('gray.300', 'gray.700'),
        bg: useColorModeValue('white', 'gray.900'),
        bgDarker: useColorModeValue('gray.300', 'gray.700'),
        lighten: useColorModeValue('gray.500', 'gray.400'),
    };
}
