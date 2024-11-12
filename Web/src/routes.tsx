import { IconType } from 'react-icons';
import {
    IoCube,
    IoCubeOutline,
    IoFolderOpen,
    IoFolderOpenOutline,
    IoHome,
    IoHomeOutline,
    IoListCircle,
    IoListCircleOutline,
    IoPeople,
    IoPeopleOutline,
    IoSettingsOutline,
    IoSettingsSharp,
} from 'react-icons/io5';
import { Navigate, RouteObject } from 'react-router-dom';
import { Login } from './components/pages/account/Login';
import { Register } from './components/pages/account/Register';
import { Token } from './components/pages/account/Token';
import { GoRedirect } from './components/pages/GoRedirect';
import { Groups } from './components/pages/groups/Groups';
import { GroupsCreate } from './components/pages/groups/GroupsCreate';
import { GroupsDetail } from './components/pages/groups/GroupsDetail';
import { GroupsEdit } from './components/pages/groups/GroupsEdit';
import { Home } from './components/pages/home/Home';
import { OrganizationsEdit } from './components/pages/organization/OrganizationsEdit';
import { Player } from './components/pages/Player';
import { PlaylistCreate } from './components/pages/playlists/PlaylistCreate';
import { PlaylistDetail } from './components/pages/playlists/PlaylistDetail';
import { PlaylistEdit } from './components/pages/playlists/PlaylistEdit';
import { PlaylistGallery } from './components/pages/playlists/PlaylistGallery';
import { PlaylistList } from './components/pages/playlists/PlaylistList';
import { AuthorDetail } from './components/pages/projects/authors/AuthorDetail';
import { CreateProject } from './components/pages/projects/CreateProject';
import { ProjectDetail } from './components/pages/projects/ProjectDetail';
import { ProjectEdit } from './components/pages/projects/ProjectEdit';
import { Projects } from './components/pages/projects/Projects';
import { AccountRoot } from './components/pages/root/AccountRoot';
import { AuthRoot } from './components/pages/root/AuthRoot';
import { OrganizationRedirect } from './components/pages/root/OrganizationRedirect';
import { Root } from './components/pages/root/Root';
import { UnauthRoot } from './components/pages/root/UnauthRoot';
import { ServerError } from './components/pages/ServerError';
import { SystemComponent } from './components/pages/system/SystemComponent';
import { OutletOrChildren } from './components/utils/OutletOrChildren';
import { Status } from './components/utils/Status';
import { Organization } from './data/Organization';
import { User } from './data/User';
import { Permission } from './schemas/generic';

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
                element: <OrganizationRedirect />,
                children: [
                    {
                        path: ':organization',
                        element: <AuthRoot />,
                        children: authRoutes(t),
                    },
                ],
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
    {
        path: '/error',
        element: <ServerError />,
    },
];

export const authRoutes = (
    t: (id: string) => string,
    user?: User | null,
    currentOrganization?: Organization,
): AppRoute[] => [
    {
        path: '',
        title: t('route.home.title'),
        element: <Home />,
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
            default: IoCubeOutline,
            selected: IoCube,
        },
        children: [
            {
                path: 'authors/:id',
                title: t('route.projects.authors.title'),
                element: <AuthorDetail />,
            },
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
                path: 'create',
                title: t('route.groups.create.title'),
                element: <GroupsCreate />,
            },
            {
                path: ':id',
                title: t('route.groups.detail.title'),
                element: <GroupsDetail />,
                children: [
                    {
                        path: 'create',
                        title: t('route.groups.createProject.title'),
                        element: <CreateProject />,
                    },
                    {
                        path: 'edit',
                        title: t('route.groups.edit.title'),
                        element: <GroupsEdit />,
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
    {
        path: 'organization',
        title: t('route.organizations.title'),
        element: <OrganizationsEdit />,
        inMenu: (['append', 'write', 'all'] as Permission[]).some((perm) =>
            currentOrganization?.userPermissions?.includes(perm),
        ),
        icon: {
            default: IoPeopleOutline,
            selected: IoPeople,
        },
        // children: [
        //     {
        //         path: 'create',
        //         title: t('route.organizations.create.title'),
        //         element: <OrganizationsCreate />,
        //     },
        //     {
        //         path: ':id',
        //         title: '',
        //         element: (
        //             <OutletOrChildren>
        //                 <Navigate to="edit" replace />
        //             </OutletOrChildren>
        //         ),
        //         children: [
        //             {
        //                 path: 'edit',
        //                 title: t('route.organizations.edit.title'),
        //                 element: <OrganizationsEdit />,
        //             },
        //         ],
        //     },
        // ],
    },
    {
        path: 'system',
        title: t('route.system.title'),
        element: <SystemComponent />,
        inMenu: (['read', 'append', 'inspect', 'write', 'all'] as Permission[]).some((perm) =>
            user?.permissions['system']?.includes(perm),
        ),
        icon: {
            default: IoSettingsOutline,
            selected: IoSettingsSharp,
        },
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
        element: <Navigate to="/account/login" />,
    },
];

export const playlistChildRoutes = (t: (id: string) => string): AppRoute[] => [
    {
        path: 'create',
        title: t('route.playlists.create.title'),
        element: <PlaylistCreate />,
    },
    {
        path: ':id',
        title: t('route.playlists.detail.title'),
        element: <PlaylistDetail />,
        children: [
            {
                path: 'edit',
                title: t('route.playlists.edit.title'),
                element: <PlaylistEdit />,
            },
            {
                path: ':itemId',
                title: t('route.playlists.detail.title'),
                element: <PlaylistDetail />,
                children: [],
            },
        ],
    },
];
