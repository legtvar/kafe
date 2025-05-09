import { Box, Center, Heading, Text } from '@chakra-ui/react';
import { t } from 'i18next';
import { components } from '../../schemas/api';
import { Brand } from '../brand/Brand';
import { ErrorContent } from './ErrorContent';

interface IErrorProps {
    error: Error | components['schemas']['KafeProblemDetails'];
}

export function Error(props: IErrorProps) {
    console.warn(props.error);
    return (
        <>
            <Box textAlign="center" py={10} px={6}>
                <Center color={'orange.300'} fontSize={'10em'}>
                    <Brand variant="cracked" />
                </Center>
                <Heading as="h2" size="xl" mt={4} mb={2}>
                    {t('error.title').toString()}
                </Heading>
                <Text color={'gray.500'}>{t('error.subtitle').toString()}</Text>
            </Box>
            <ErrorContent error={props.error} />
        </>
    );
}
