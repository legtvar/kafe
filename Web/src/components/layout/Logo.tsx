import { Center, HStack, HTMLChakraProps, Text } from '@chakra-ui/react';
import { Brand } from '../brand/Brand';

interface ILogoProps extends HTMLChakraProps<'div'> {}

export function Logo(props: ILogoProps) {
    return (
        <HStack {...props}>
            <Center display="inline-flex" fontSize="3xl" pr={2}>
                <Brand variant="stripe" />
            </Center>
            <Text fontSize="3xl" fontWeight="bold">
                KAFE
            </Text>
        </HStack>
    );
}
