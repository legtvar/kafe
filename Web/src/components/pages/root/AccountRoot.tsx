import { Flex, Stack, useColorModeValue } from '@chakra-ui/react';
import { Outlet } from 'react-router-dom';
import { useReloadVar } from '../../../hooks/useReload';
import { LanguageToggle } from '../../utils/LanguageToggle';

interface IAccountRootProps {}

export function AccountRoot(props: IAccountRootProps) {
    const { reload, value } = useReloadVar();

    return (
        <>
            <LanguageToggle
                aria-label="Toggle language"
                position="fixed"
                top={8}
                right={8}
                onLanguageToggled={() => reload()}
            />
            <Flex
                py={10}
                minH={'100vh'}
                align={'center'}
                justify={'center'}
                bg={useColorModeValue('gray.50', 'gray.800')}
            >
                <Stack spacing={8} mx={'auto'} w={{ base: '100%', md: 'lg' }} py={12} px={6}>
                    <Outlet key={value ? 'a' : 'b'} />
                </Stack>
            </Flex>
        </>
    );
}
