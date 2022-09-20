import {
    MenuFoldOutlined,
    MenuUnfoldOutlined,
    UploadOutlined,
    UserOutlined,
    VideoCameraOutlined,
} from '@ant-design/icons';
import { Layout, Menu, PageHeader, Typography } from 'antd';
import ErrorBoundary from 'antd/lib/alert/ErrorBoundary';
import React, { useEffect, useState } from 'react';
import { Trans, useTranslation } from 'react-i18next';
import { Lemmipsum } from './components/utils/Lemmipsum';

export const App: React.FC = () => {
    const [collapsed, setCollapsed] = useState(false);
    const [isSmallScreen, setIsSmallScreen] = useState(false);

    const handleWindowResize = () => {
        setIsSmallScreen(window.innerWidth < 992); // lg
    };

    useEffect(() => {
        window.addEventListener('resize', handleWindowResize);
        return () => window.removeEventListener('resize', handleWindowResize);
    });

    const { t } = useTranslation();

    return (
        <Layout style={{ minHeight: '100vh' }}>
            <Layout.Sider
                breakpoint="lg"
                collapsible
                collapsed={collapsed}
                trigger={null}
                onCollapse={(value) => setCollapsed(value)}
                collapsedWidth={isSmallScreen ? '0' : undefined}
                width={250}
            >
                <div className="kafe-logo">Kafe</div>
                <Menu
                    theme="dark"
                    mode="inline"
                    defaultSelectedKeys={['4']}
                    items={[UserOutlined, VideoCameraOutlined, UploadOutlined, UserOutlined].map((icon, index) => ({
                        key: String(index + 1),
                        icon: React.createElement(icon),
                        label: `nav ${index + 1}`,
                    }))}
                />
            </Layout.Sider>
            <Layout>
                <Layout.Header className="kafe-layout-header">
                    <PageHeader
                        backIcon={React.createElement(collapsed ? MenuUnfoldOutlined : MenuFoldOutlined, {
                            className: 'trigger',
                        })}
                        onBack={() => setCollapsed(!collapsed)}
                        title="Title"
                        subTitle="This is a subtitle"
                    />
                </Layout.Header>
                <Layout.Content style={{ margin: '24px 16px 0' }}>
                    <ErrorBoundary>
                        <div className="kafe-layout-content">
                            <Typography.Paragraph>
                                <Lemmipsum />
                            </Typography.Paragraph>
                            <Typography.Paragraph>
                                <Lemmipsum />
                            </Typography.Paragraph>
                            <Typography.Paragraph>
                                <Lemmipsum />
                            </Typography.Paragraph>
                            <Typography.Paragraph>
                                <Lemmipsum />
                            </Typography.Paragraph>
                            <Typography.Paragraph>
                                <Lemmipsum />
                            </Typography.Paragraph>
                        </div>
                    </ErrorBoundary>
                </Layout.Content>
                <Layout.Footer className="kafe-layout-footer">
                    <span className="kafe-footer-content">
                        <Trans i18nKey="layout.footer.copy">
                            Created with <span className="kafe-heart">❤️</span> and ☕ by
                            <a href="https://lemma.fi.muni.cz/" target="_blank" rel="noreferrer">
                                LEMMA
                            </a>
                        </Trans>{' '}
                        &copy; 2022 - {new Date().getFullYear()}
                    </span>
                </Layout.Footer>
            </Layout>
        </Layout>
    );
};

export default App;
