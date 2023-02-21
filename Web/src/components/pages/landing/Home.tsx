import { Box } from '@chakra-ui/react';
import { useReload } from '../../../hooks/useReload';
import { Navbar } from '../../layout/navigation/Navbar';
import { Playlists } from '../playlists/Playlists';

interface IHomeProps {}

export function Home(props: IHomeProps) {
    const reload = useReload();

    return (
        <>
            <Navbar forceReload={() => reload()} signedIn={false} />
            <Box w="100%" mt={20} p={6} fontSize="xl">
                <Playlists />
            </Box>
        </>
    );
}
