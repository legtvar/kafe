import { Box, HStack, Text, useColorModeValue } from '@chakra-ui/react';
import * as React from 'react';
import { BsCheckCircleFill, BsCircle, BsFillExclamationCircleFill } from 'react-icons/bs';
import { useColorScheme } from '../../../hooks/useColorScheme';

interface IStatusCheckProps {
    children: React.ReactNode;
    status: 'ok' | 'nok' | 'unknown';
    details?: React.ReactNode;
}

export function StatusCheck({ children, status, details }: IStatusCheckProps) {
    const icon = {
        ok: {
            icon: <BsCheckCircleFill />,
            color: 'green.500',
        },
        nok: {
            icon: <BsFillExclamationCircleFill />,
            color: 'red.500',
        },
        unknown: {
            icon: <BsCircle />,
            color: useColorModeValue('gray.300', 'gray.700'),
        },
    }[status];

    const { lighten } = useColorScheme();

    return (
        <>
            <HStack spacing={4} alignItems="center" my={4}>
                <Text fontSize={'2.5em'} color={icon.color}>
                    {icon.icon}
                </Text>
                <Text>{children}</Text>
            </HStack>
            {details && (
                <Box pl={'calc(40px + 1rem)'} mb={6} mt={-2} color={lighten}>
                    {details}
                </Box>
            )}
        </>
    );
}
