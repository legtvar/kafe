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
import { PageHome } from './pages/Home';

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
        element: <PageHome />,
        inMenu: true,
        icon: {
            default: IoHomeOutline,
            selected: IoHome,
        },
    },
    {
        path: 'projects',
        title: t('route.projects.title'),
        element: <>Lorem ipsum</>,
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
        element: <>Tady bude seznam playlistů</>,
        inMenu: true,
        icon: {
            default: IoListCircleOutline,
            selected: IoListCircle,
        },
    },
];
