import { Box, useColorModeValue } from '@chakra-ui/react';
import { components } from '../../schemas/api';

interface IErrorContentProps {
    error: Error | components['schemas']['KafeProblemDetails'];
}

export function ErrorContent(props: IErrorContentProps) {
    let title = 'An error occurred';
    let description: string | null = null;
    let suberrors: components['schemas']['Error'][] = [];

    if ((props.error as unknown as any).title) {
        const kpd = props.error as components['schemas']['KafeProblemDetails'];
        title = kpd?.title ?? title;
        description = kpd?.detail ?? description;
        suberrors = kpd?.errors ?? [];
    } else {
        const jsError = props.error as Error;
        title = 'A client error occurred';
        suberrors = [
            {
                id: jsError.name,
                message: jsError.message,
                arguments: {},
                stackTrace: jsError.stack,
            },
        ];
    }

    return (
        <>
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
                {description}
            </Box>

            {suberrors.map((e) => (
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
                    <strong>
                        {e.id}: {e.message}
                    </strong>
                    <br />
                    {e.stackTrace}
                </Box>
            ))}
        </>
    );
}
