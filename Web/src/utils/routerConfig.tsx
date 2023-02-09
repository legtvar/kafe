import { RouteObject } from 'react-router-dom';
import { Root } from '../components/layout/Root';
import { Status } from '../components/layout/Status';
import { routes } from '../routes';

export const routerConfig = (t: (id: string) => string): RouteObject[] => [
    {
        path: '/',
        element: <Root />,
        errorElement: <Status />,
        children: routes(t),
    },
];
