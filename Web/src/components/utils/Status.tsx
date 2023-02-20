import { Box, Button, Center, Heading, Text } from '@chakra-ui/react';
import { t } from 'i18next';
import React from 'react';
import { useLinkClickHandler, useRouteError } from 'react-router-dom';
import { Brand } from '../brand/Brand';

export interface IStatusProps {
    error?:
        | string
        | {
              status: string | number;
              statusText: string;
              message?: string;
          };
}

export const Status: React.FC<IStatusProps> = (props: IStatusProps) => {
    let error = useRouteError() as any;
    const backlink = useLinkClickHandler('/');

    if (props.error) {
        error = {
            status: 'error',
            statusText: props.error,
        };
    }

    console.error(error);

    return (
        <Center w="100vw" h="100vh">
            <Box textAlign="center" py={10} px={6}>
                <Heading display="block" fontSize="15rem" color="brand.500">
                    <Brand variant="broken" />
                </Heading>
                <Heading display="block" as="h2" size="4xl" bg="brand.500" backgroundClip="text">
                    {error.status}
                </Heading>
                <Text fontSize="18px" mt={3} mb={2}>
                    {t('status.404.title').toString()}
                </Text>
                <Text color={'gray.500'} mb={6}>
                    {t('status.404.subtitle').toString()}
                </Text>

                <Button color="white" variant="solid" onClick={backlink as any}>
                    {t('status.backlink').toString()}
                </Button>
            </Box>
        </Center>
    );
};
