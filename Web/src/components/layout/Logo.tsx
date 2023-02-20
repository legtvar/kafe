import { HStack, HTMLChakraProps, Text } from '@chakra-ui/react';
import { Brand } from '../brand/Brand';

interface ILogoProps extends HTMLChakraProps<'div'> {}

export function Logo(props: ILogoProps) {
    return (
        <HStack {...props}>
            <Text fontSize="3xl" fontFamily="monospace" fontWeight="bold">
                KAFE
            </Text>
            <Text fontSize="2xl" pl={2}>
                <Brand variant="stripe" />
            </Text>
        </HStack>
    );
}
