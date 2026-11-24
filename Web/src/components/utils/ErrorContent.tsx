import { Box, useColorModeValue } from '@chakra-ui/react';
import { t } from 'i18next';
import { components } from '../../schemas/api';

interface IErrorContentProps {
    error: Error | components['schemas']['KafeProblemDetails'];
}

export function ErrorContent(props: IErrorContentProps) {
    let title = t('sendApi.error').toString();
    let description: string | null = null;
    let suberrors: components['schemas']['Error'][] = [];

    if ((props.error as unknown as any)?.title) {
        const kpd = props.error as components['schemas']['KafeProblemDetails'];
        title = kpd?.title ?? title;
        description = kpd?.detail ?? description;
        suberrors = kpd?.errors ?? [];
    } else {
        title = t('error.client').toString();
        const jsError = props.error as Error;
        if (jsError)
        {
            suberrors = [
                {
                    id: jsError.name,
                    message: jsError.message,
                    arguments: {},
                    stackTrace: jsError.stack,
                },
            ];
        }
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
                    key={e.id}
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
