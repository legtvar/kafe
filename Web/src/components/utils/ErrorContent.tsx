import { Box, useColorModeValue } from '@chakra-ui/react';
import { components } from '../../schemas/api';

interface IErrorContentProps {
    error: any;
}

export function ErrorContent(props: IErrorContentProps) {
    let title = 'API error';
    let description: string | null = null;

    const kpd = props.error as components['schemas']['KafeProblemDetails'];
    title = kpd?.title ?? title;
    description = kpd?.detail ?? description;

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

            {kpd?.errors.map((e) => (
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
