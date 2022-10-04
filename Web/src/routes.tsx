import { GroupOutlined, HomeOutlined, PlaySquareOutlined, VideoCameraOutlined } from '@ant-design/icons';

export type AppRoute = {
    path: string;
    title: string;
    subtitle?: string;
    icon?: React.ReactNode;
    element: React.ReactNode;
    inMenu?: boolean;
    children?: AppRoute[];
};

export const routes = (t: (id: string) => string): AppRoute[] => [
    {
        path: '',
        title: t('route.home.title'),
        element: <>Lorem ipsum</>,
        inMenu: true,
        icon: <HomeOutlined />,
    },
    {
        path: 'projects',
        title: t('route.projects.title'),
        element: <>Tady bude seznam projektů</>,
        inMenu: true,
        icon: <VideoCameraOutlined />,
    },
    {
        path: 'groups',
        title: t('route.groups.title'),
        element: <>Tady bude seznam skupin projektů</>,
        inMenu: true,
        icon: <GroupOutlined />,
    },
    {
        path: 'playlists',
        title: t('route.playlists.title'),
        element: <>Tady bude seznam playlistů</>,
        inMenu: true,
        icon: <PlaySquareOutlined />,
    },
];
