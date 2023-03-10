import { Box, useColorModeValue } from '@chakra-ui/react';
import { components } from '../../schemas/api';

interface IErrorContentProps {
    error: any;
}

export function ErrorContent(props: IErrorContentProps) {
    let title = '';
    let description = '';

    if (props.error.title) {
        const as400Response = props.error as components['schemas']['ProblemDetails'];

        title = as400Response.title || 'API error';
        description = as400Response.detail! || '';
    } else {
        title = props.error.toString();
        description = props.error.stack.toString();
    }

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
            <strong>{title}</strong>
            <br />
            <br />
            {description}
        </Box>
    );
}
