import { Box, useColorMode } from '@chakra-ui/react';
import { useReload } from '../../../hooks/useReload';
import { Navbar } from '../../layout/navigation/Navbar';
import { PublicPlaylists } from '../playlists/PublicPlaylists';

interface IHomeProps {}

export function Home(props: IHomeProps) {
    const reload = useReload();
    const { colorMode, toggleColorMode } = useColorMode();

    return (
        <>
            <Navbar forceReload={() => reload()} signedIn={false} />
            <Box w="100%" mt={20} p={6} fontSize="xl">
                <PublicPlaylists />
            </Box>
        </>
    );
}
