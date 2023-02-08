import { Menu } from 'antd';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useMatches } from 'react-router-dom';
import { AppRoute, routes } from '../../routes';

export const PageMenu: React.FC = () => {
    const matches = useMatches();
    const i18next = useTranslation();

    const routeValues = routes(i18next.t);

    const match = matches[matches.length - 1].id
        .split('-')
        .slice(1)
        .reduce(
            ({ route, path }, id) => ({
                route: route.children![parseInt(id)],
                path: path + '/' + route.children![parseInt(id)].path,
            }),
            { route: { children: routeValues, path: '' } as AppRoute, path: '' },
        );

    const mapper =
        (path: string) =>
        (route: AppRoute, i: number): React.ReactNode => {
            const fullPath = path + '/' + route.path;
            const children = route.children?.filter((route) => route.inMenu).map(mapper(fullPath));

            return (
                <>
                    <Menu.Item key={i}>
                        {route.icon}
                        <span>{route.title}</span>
                        <Link to={fullPath} />
                    </Menu.Item>
                    {match.path.startsWith(fullPath) && children && children.length > 0 && (
                        <Menu
                            theme="dark"
                            mode="inline"
                            selectedKeys={[match.path]}
                            style={{ backgroundColor: '#000c17' }}
                            key={i + '_sub'}
                        >
                            {children}
                        </Menu>
                    )}
                </>
            );
        };

    const items = routeValues.filter((route) => route.inMenu).map(mapper(''));

    return (
        <Menu theme="dark" mode="inline" selectedKeys={[match.path]}>
            {items}
        </Menu>
    );
};
