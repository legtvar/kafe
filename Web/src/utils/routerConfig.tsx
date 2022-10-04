import { RouteObject } from 'react-router-dom';
import { Error } from '../components/layout/Error';
import { Page } from '../components/layout/Page';
import { Root } from '../components/layout/Root';
import { AppRoute, routes } from '../routes';

function routeMapper(route: AppRoute): RouteObject {
    return {
        path: route.path,
        element: (
            <Page title={route.title} subtitle={route.subtitle}>
                {route.element}
            </Page>
        ),
        children: route.children ? route.children.map(routeMapper) : undefined,
    };
}

export const routerConfig = (t: (id: string) => string): RouteObject[] => [
    {
        path: '/',
        element: <Root />,
        errorElement: <Error />,
        children: routes(t).map(routeMapper),
    },
];
