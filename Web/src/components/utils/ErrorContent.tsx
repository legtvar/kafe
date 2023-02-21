import { Box, useColorModeValue } from '@chakra-ui/react';

interface IErrorContentProps {
    error: any;
}

export function ErrorContent(props: IErrorContentProps) {
    return (
        <Box
            borderRadius={'lg'}
            border="1px"
            borderColor={useColorModeValue('orange.300', 'orange.800')}
            bg={useColorModeValue('orange.200', 'orange.900')}
            px={5}
            py={4}
            whiteSpace="pre"
            fontFamily="consolas, monospace"
            overflow="auto"
            mb={10}
        >
            <strong>{props.error.toString()}</strong>
            <br />
            <br />
            {props.error.stack.toString()}
        </Box>
    );
}
