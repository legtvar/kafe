import { RouteObject } from 'react-router-dom';
import { Error } from '../components/layout/Error';
import { Root } from '../components/layout/Root';
import { routes } from '../routes';

export const routerConfig = (t: (id: string) => string): RouteObject[] => [
    {
        path: '/',
        element: <Root />,
        errorElement: <Error />,
        children: routes(t),
    },
];
