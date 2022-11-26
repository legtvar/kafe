import { GroupOutlined, HomeOutlined, PlaySquareOutlined, VideoCameraOutlined } from '@ant-design/icons';
import { ProjectDetailWrapper } from './pages/projects/ProjectDetailWrapper';
import { ProjectsPage } from './pages/projects/ProjectsPage';

export type AppRoute = {
    path: string;
    title: string;
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
        element: <ProjectsPage />,
        inMenu: true,
        icon: <VideoCameraOutlined />,
        children: [
            {
                path: ':id',
                title: t('route.projects.detail.title'),
                element: <ProjectDetailWrapper />,
            },
        ],
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
