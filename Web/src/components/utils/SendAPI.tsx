import { Box, Text, useColorModeValue, useToast } from '@chakra-ui/react';
import { t } from 'i18next';
import { useState } from 'react';
import { API, ApiResponse } from '../../api/API';
import { useApi } from '../../hooks/Caffeine';
import { components } from '../../schemas/api';
import { HRIB } from '../../schemas/generic';

type SendAPIStatus = 'ready' | 'sending' | 'error' | 'ok';

export interface ISendAPIProps<T> {
    request: (api: API, data: T) => Promise<ApiResponse<HRIB>>;
    value: T;
    children: (
        onSubmit: () => void,
        status: SendAPIStatus,
        error: components['schemas']['ProblemDetails'] | null,
    ) => JSX.Element;
    onSubmited: (id: HRIB) => void;
    repeatable?: boolean;
}

export function SendAPI<T>({ request, value, children, onSubmited, repeatable }: ISendAPIProps<T>) {
    const api = useApi();
    const [status, setStatus] = useState<SendAPIStatus>('ready');
    const [error, setError] = useState<components['schemas']['ProblemDetails'] | null>(null);
    const border = useColorModeValue('orange.300', 'orange.800');
    const bg = useColorModeValue('orange.200', 'orange.900');
    const toast = useToast();

    const onSubmit = async () => {
        if (!(status === 'ready' || status === 'error')) return;
        setStatus('sending');
        const result = await request(api, value);

        if (result.status === 200) {
            if (repeatable) {
                setStatus('ready');
            } else {
                setStatus('ok');
            }
            onSubmited(result.data);
        } else {
            setStatus('error');
            console.log(result);
            setError(result.error);
            toast({
                title: t('sendApi.error').toString(),
                description: (
                    <>
                        {typeof result.error === 'string' && (
                            <Text>
                                {(result.error as string)
                                    .split('\n')[0]
                                    .replace('System.ArgumentException: ', '')
                                    .replace(" (Parameter 'dto')", '')}
                            </Text>
                        )}
                        {result.error?.errors ? (
                            Object.entries(result.error.errors).map(([field, problem]: any, i) => (
                                <Text key={i}>
                                    <strong>{field}: </strong>
                                    {problem}
                                </Text>
                            ))
                        ) : (
                            <Text>{result.error!.title}</Text>
                        )}
                    </>
                ),
                status: 'error',
                duration: 9000,
                isClosable: true,
            });
        }
    };

    return (
        <Box>
            {/* {status === 'error' && (
                <>
                    <Box borderRadius={'lg'} border="1px" borderColor={border} bg={bg} px={5} py={4} mb={8}>
                        <Heading as="h4" fontSize="lg" pb={4}>
                            {t('sendApi.error').toString()}
                        </Heading>
                        {typeof error === 'string' && (
                            <Text>
                                {(error as string)
                                    .split('\n')[0]
                                    .replace('System.ArgumentException: ', '')
                                    .replace(" (Parameter 'dto')", '')}
                            </Text>
                        )}
                        {error?.errors ? (
                            Object.entries(error.errors).map(([field, problem]: any, i) => (
                                <Text key={i}>
                                    <strong>{field}: </strong>
                                    {problem}
                                </Text>
                            ))
                        ) : (
                            <Text>{error!.title}</Text>
                        )}
                    </Box>
                    <Spacer />
                </>
            )} */}
            {children(onSubmit, status, error)}
        </Box>
    );
}
