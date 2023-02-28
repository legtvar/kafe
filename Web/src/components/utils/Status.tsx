import { Box, Button, Center, Heading, Text } from '@chakra-ui/react';
import { t } from 'i18next';
import React from 'react';
import { useLinkClickHandler, useRouteError } from 'react-router-dom';
import { Brand } from '../brand/Brand';

export interface IStatusProps {
    statusCode?: string | number;
    embeded?: true; // !standalone
}

export const Status: React.FC<IStatusProps> = (props: IStatusProps) => {
    const routeError = useRouteError() as any;
    let statusCode = routeError?.status;
    const backlink = useLinkClickHandler('/');

    if (props.statusCode) {
        statusCode = props.statusCode;
    }

    routeError && console.error(routeError);

    return (
        <Center w={props.embeded ? '100%' : '100vw'} h={props.embeded ? '100%' : '100vh'}>
            <Box textAlign="center" py={10} px={6}>
                <Heading display="block" fontSize="15rem" color="brand.500">
                    <Brand variant="broken" />
                </Heading>
                <Heading display="block" as="h2" size="4xl" bg="brand.500" backgroundClip="text">
                    {statusCode}
                </Heading>
                <Text fontSize="18px" mt={3} mb={2}>
                    {t(`status.${statusCode}.title`).toString()}
                </Text>
                <Text color={'gray.500'} mb={6}>
                    {t(`status.${statusCode}.subtitle`).toString()}
                </Text>

                <Button variant="solid" onClick={backlink as any}>
                    {t('status.backlink').toString()}
                </Button>
            </Box>
        </Center>
    );
};
