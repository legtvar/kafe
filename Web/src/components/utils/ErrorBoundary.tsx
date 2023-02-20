import { Box, Center, Heading, Text } from '@chakra-ui/react';
import { t } from 'i18next';
import * as React from 'react';
import { IoWarning } from 'react-icons/io5';
import { ErrorContent } from './ErrorContent';

interface IErrorBoundaryProps {
    children: React.ReactNode;
}

interface IErrorBoundaryState {
    error: any;
}

export class ErrorBoundary extends React.Component<IErrorBoundaryProps, IErrorBoundaryState> {
    state: IErrorBoundaryState = {
        error: null,
    };

    static getDerivedStateFromError(error: any) {
        return { error };
    }

    render() {
        if (this.state.error) {
            return (
                <>
                    <Box textAlign="center" py={10} px={6}>
                        <Center color={'orange.300'} fontSize={'70px'}>
                            <IoWarning />
                        </Center>
                        <Heading as="h2" size="xl" mt={4} mb={2}>
                            {t('error.title').toString()}
                        </Heading>
                        <Text color={'gray.500'}>{t('error.subtitle').toString()}</Text>
                    </Box>
                    <ErrorContent error={this.state.error} />
                </>
            );
        }
        return this.props.children;
    }
}
