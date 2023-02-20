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
import { PublicPlaylists } from './components/pages/playlists/PublicPlaylists';
import { PageProjects } from './components/pages/Projects';

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

export const routes = (t: (id: string) => string): AppRoute[] => [
    {
        path: '',
        title: t('route.home.title'),
        element: <PublicPlaylists />,
        inMenu: true,
        icon: {
            default: IoHomeOutline,
            selected: IoHome,
        },
    },
    {
        path: 'projects',
        title: t('route.projects.title'),
        element: <PageProjects />,
        inMenu: true,
        icon: {
            default: IoVideocamOutline,
            selected: IoVideocam,
        },
        children: [
            {
                path: ':id',
                title: t('route.projects.detail.title'),
                element: <>Lorem ipsum</>,
            },
        ],
    },
    {
        path: 'groups',
        title: t('route.groups.title'),
        element: <>Tady bude seznam skupin projektů</>,
        inMenu: true,
        icon: {
            default: IoFolderOpenOutline,
            selected: IoFolderOpen,
        },
    },
    {
        path: 'playlists',
        title: t('route.playlists.title'),
        element: <>Playlists</>,
        inMenu: true,
        icon: {
            default: IoListCircleOutline,
            selected: IoListCircle,
        },
    },
];
