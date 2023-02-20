import { RouteObject } from 'react-router-dom';
import { Root } from '../components/layout/Root';
import { Login } from '../components/pages/account/Login';
import { Register } from '../components/pages/account/Register';
import { Home } from '../components/pages/landing/Home';
import { Status } from '../components/utils/Status';
import { routes } from '../routes';

export const routerConfig = (t: (id: string) => string): RouteObject[] => [
    {
        path: '/',
        element: <Home />,
        errorElement: <Status />,
    },
    {
        path: '/auth',
        element: <Root />,
        children: routes(t),
    },
    {
        path: '/login',
        element: <Login />,
    },
    {
        path: '/register',
        element: <Register />,
    },
];
