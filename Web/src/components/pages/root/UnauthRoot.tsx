import { Box } from '@chakra-ui/react';
import { Outlet } from 'react-router-dom';
import { useReload } from '../../../hooks/useReload';
import { Navbar } from '../../layout/navigation/Navbar';

interface IUnauthRootProps {}

export function UnauthRoot(props: IUnauthRootProps) {
    const reload = useReload();

    return (
        <>
            <Navbar forceReload={() => reload()} signedIn={false} />
            <Box w="100%" mt={20} p={6} fontSize="xl">
                <Outlet />
            </Box>
        </>
    );
}
