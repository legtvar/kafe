import { IconType } from 'react-icons';
import {
    IoFolderOpen,
    IoFolderOpenOutline,
    IoHome,
    IoHomeOutline,
    IoListCircle,
    IoListCircleOutline,
    IoVideocam,
    IoVideocamOutline,
} from 'react-icons/io5';
import { Navigate, RouteObject } from 'react-router-dom';
import { Login } from './components/pages/account/Login';
import { Register } from './components/pages/account/Register';
import { TempAccount } from './components/pages/account/TempAccount';
import { Token } from './components/pages/account/Token';
import { GoRedirect } from './components/pages/GoRedirect';
import { Groups } from './components/pages/groups/Groups';
import { GroupsDetail } from './components/pages/groups/GroupsDetail';
import { HomeFestival } from './components/pages/home/HomeFestival';
import { Player } from './components/pages/Player';
import { PlaylistDetail } from './components/pages/playlists/PlaylistDetail';
import { PlaylistGallery } from './components/pages/playlists/PlaylistGallery';
import { PlaylistList } from './components/pages/playlists/PlaylistList';
import { CreateProject } from './components/pages/projects/CreateProject';
import { ProjectDetail } from './components/pages/projects/ProjectDetail';
import { ProjectEdit } from './components/pages/projects/ProjectEdit';
import { Projects } from './components/pages/projects/Projects';
import { AccountRoot } from './components/pages/root/AccountRoot';
import { AuthRoot } from './components/pages/root/AuthRoot';
import { Root } from './components/pages/root/Root';
import { UnauthRoot } from './components/pages/root/UnauthRoot';
import { OutletOrChildren } from './components/utils/OutletOrChildren';
import { Status } from './components/utils/Status';

export type SelectableIcon = {
    default: IconType;
    selected?: IconType;
};

export type AppRoute = {
    path: string;
    title: string;
    icon?: SelectableIcon;
    element: React.ReactNode;
    inMenu?: boolean;
    children?: AppRoute[];
};

export const routerConfig = (t: (id: string) => string): RouteObject[] => [
    {
        path: '/',
        element: <Root />,
        errorElement: <Status />,
        children: [
            {
                path: 'home',
                element: <UnauthRoot />,
                errorElement: <Status />,
                children: unauthRoutes(t),
            },
            {
                path: 'auth',
                element: <AuthRoot />,
                children: authRoutes(t),
            },
            {
                path: 'account',
                element: <AccountRoot />,
                children: accountRoutes(t),
            },
        ],
    },
    {
        path: '/account/token/:token',
        element: <Token />,
    },
    {
        path: '/go/:slug',
        element: <GoRedirect />,
    },
    {
        path: '/play/:slug',
        element: <Player />,
    },
];

export const authRoutes = (t: (id: string) => string): AppRoute[] => [
    {
        path: '',
        title: t('route.home.title'),
        element: <HomeFestival />,
        inMenu: true,
        icon: {
            default: IoHomeOutline,
            selected: IoHome,
        },
    },
    {
        path: 'projects',
        title: t('route.projects.title'),
        element: <Projects />,
        inMenu: true,
        icon: {
            default: IoVideocamOutline,
            selected: IoVideocam,
        },
        children: [
            {
                path: ':id',
                title: t('route.projects.detail.title'),
                element: <ProjectDetail />,
                children: [
                    {
                        path: 'edit',
                        title: t('route.projects.detail.title'),
                        element: <ProjectEdit />,
                    },
                ],
            },
        ],
    },
    {
        path: 'groups',
        title: t('route.groups.title'),
        element: <Groups />,
        inMenu: true,
        icon: {
            default: IoFolderOpenOutline,
            selected: IoFolderOpen,
        },
        children: [
            {
                path: ':id',
                title: t('route.groups.detail.title'),
                element: <GroupsDetail />,
                children: [
                    {
                        path: 'create',
                        title: t('route.groups.create.title'),
                        element: <CreateProject />,
                    },
                ],
            },
        ],
    },
    {
        path: 'playlists',
        title: t('route.playlists.title'),
        element: <PlaylistList />,
        inMenu: true,
        icon: {
            default: IoListCircleOutline,
            selected: IoListCircle,
        },
        children: playlistChildRoutes(t),
    },
];

export const unauthRoutes = (t: (id: string) => string): RouteObject[] => [
    {
        path: '',
        element: <PlaylistGallery />,
    },
    {
        path: 'playlists',
        element: (
            <OutletOrChildren>
                <Navigate to="/" />
            </OutletOrChildren>
        ),
        children: playlistChildRoutes(t),
    },
];

export const accountRoutes = (t: (id: string) => string): RouteObject[] => [
    {
        path: 'login',
        element: <Login />,
    },
    {
        path: 'register',
        element: <Register />,
    },
    {
        path: 'temp',
        element: <TempAccount />,
    },
];

export const playlistChildRoutes = (t: (id: string) => string): AppRoute[] => [
    {
        path: ':id',
        title: t('route.playlists.detail.title'),
        element: <PlaylistDetail />,
    },
];
