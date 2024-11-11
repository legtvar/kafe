import { Box, Button, Center, Heading, Text } from '@chakra-ui/react';
import i18next, { t } from 'i18next';
import React from 'react';
import { useLinkClickHandler, useRouteError } from 'react-router-dom';
import { Brand } from '../brand/Brand';

export interface IStatusProps {
    statusCode?: string | number;
    embeded?: true; // !standalone
    log?: any;
    noButton?: true;
}

export const Status: React.FC<IStatusProps> = (props: IStatusProps) => {
    const routeError = useRouteError() as any;
    let statusCode = routeError?.status;
    const backlink = useLinkClickHandler('/');

    console.log(statusCode);

    if (props.statusCode) {
        statusCode = props.statusCode;
    }

    routeError && console.error(routeError);

    props.log && console.log(props.log);

    return (
        <Center w={props.embeded ? '100%' : '100vw'} h={props.embeded ? '100%' : '100vh'}>
            <Box textAlign="center" py={10} px={6}>
                <Heading display="block" fontSize="15rem" color="brand.500">
                    <Brand variant="broken" />
                </Heading>
                <Heading display="block" as="h2" size="4xl" color="brand.500" mb={4}>
                    {i18next.exists(`status.${statusCode}.code`)
                        ? t(`status.${statusCode}.code`).toString()
                        : statusCode}
                </Heading>
                {i18next.exists(`status.${statusCode}.title`) && (
                    <Text fontSize="18px" pt={3} pb={2}>
                        {t(`status.${statusCode}.title`).toString()}
                    </Text>
                )}
                {i18next.exists(`status.${statusCode}.subtitle`) && (
                    <Text color={'gray.500'}>{t(`status.${statusCode}.subtitle`).toString()}</Text>
                )}
                {!props.noButton && (
                    <Button variant="solid" onClick={backlink as any} mt={6}>
                        {t('status.backlink').toString()}
                    </Button>
                )}
            </Box>
        </Center>
    );
};
