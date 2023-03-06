import { Center, HStack, HTMLChakraProps, Text } from '@chakra-ui/react';
import { Brand } from '../brand/Brand';

interface ILogoProps extends HTMLChakraProps<'div'> {}

export function Logo(props: ILogoProps) {
    return (
        <HStack {...props}>
            <Text fontSize="3xl" fontWeight="bold">
                KAFE
            </Text>
            <Center display="inline-flex" fontSize="3xl" pl={2}>
                <Brand variant="stripe" />
            </Center>
        </HStack>
    );
}
