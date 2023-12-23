import { Navigate } from 'react-router-dom';

interface IUnauthRootProps {}

export function UnauthRoot(props: IUnauthRootProps) {
    // const reload = useReload();

    // TODO: Temporary
    return <Navigate to={'/account/login'} />;

    // return (
    //     <>
    //         <Navbar forceReload={() => reload()} signedIn={false} />
    //         <Box w="100%" mt={20} p={6} fontSize="xl">
    //             <Outlet />
    //         </Box>
    //     </>
    // );
}
