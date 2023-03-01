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
import { CreateProject } from './components/pages/form/CreateProject';
import { GoRedirect } from './components/pages/GoRedirect';
import { Groups } from './components/pages/groups/Groups';
import { GroupsDetail } from './components/pages/groups/GroupsDetail';
import { PlaylistDetail } from './components/pages/playlists/PlaylistDetail';
import { PlaylistGallery } from './components/pages/playlists/PlaylistGallery';
import { PlaylistList } from './components/pages/playlists/PlaylistList';
import { ProjectDetail } from './components/pages/projects/ProjectDetail';
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
            {
                path: 'go',
                element: (
                    <OutletOrChildren>
                        <Navigate to="/" />
                    </OutletOrChildren>
                ),
                children: [
                    {
                        path: ':slug',
                        element: <GoRedirect />,
                    },
                ],
            },
        ],
    },
];

export const authRoutes = (t: (id: string) => string): AppRoute[] => [
    {
        path: '',
        title: t('route.home.title'),
        element: <PlaylistGallery />,
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
];

export const playlistChildRoutes = (t: (id: string) => string): AppRoute[] => [
    {
        path: ':id',
        title: t('route.playlists.detail.title'),
        element: <PlaylistDetail />,
    },
];
