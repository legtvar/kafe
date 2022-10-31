import { Layout } from 'antd';
import React, { useEffect, useState } from 'react';
import { Trans } from 'react-i18next';
import { Link, Outlet } from 'react-router-dom';
import { PageOutletContext } from './Page';
import { PageMenu } from './PageMenu';

export const Root: React.FC = () => {
    const [collapsed, setCollapsed] = useState(false);
    const [isSmallScreen, setIsSmallScreen] = useState(false);

    const handleWindowResize = () => {
        setIsSmallScreen(window.innerWidth < 992); // lg
    };

    useEffect(() => {
        handleWindowResize();
        window.addEventListener('resize', handleWindowResize);
        return () => window.removeEventListener('resize', handleWindowResize);
    });

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
                className="kafe-sider"
            >
                <Link to="/">
                    <div className="kafe-logo">Kafe</div>
                </Link>
                <PageMenu />
            </Layout.Sider>
            <Layout>
                <Outlet context={{ collapsed, setCollapsed, isSmallScreen } as PageOutletContext} />
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
