import { MenuFoldOutlined, MenuUnfoldOutlined } from '@ant-design/icons';
import { Layout, PageHeader } from 'antd';
import ErrorBoundary from 'antd/lib/alert/ErrorBoundary';
import { t } from 'i18next';
import React, { useEffect } from 'react';
import { useOutlet, useOutletContext } from 'react-router-dom';

export interface IPageProps {
    title: string;
    subtitle?: string;
    children: React.ReactNode;
}

export type PageOutletContext = {
    collapsed: boolean;
    setCollapsed: (value: boolean) => void;
    isSmallScreen: boolean;
};

export const Page: React.FC<IPageProps> = (props) => {
    const outletContext = useOutletContext() as PageOutletContext;
    const { collapsed, setCollapsed } = outletContext;
    const outlet = useOutlet(outletContext);
    const { title, subtitle, children } = props;

    useEffect(() => {
        document.title = t('layout.title-prefix') + title;
    }, [title]);

    if (outlet) {
        return outlet;
    }

    return (
        <>
            <Layout.Header className="kafe-layout-header">
                <PageHeader
                    backIcon={React.createElement(collapsed ? MenuUnfoldOutlined : MenuFoldOutlined, {
                        className: 'trigger',
                    })}
                    onBack={() => setCollapsed(!collapsed)}
                    title={title}
                    subTitle={subtitle}
                />
            </Layout.Header>
            <Layout.Content style={{ margin: '24px 16px 0' }}>
                <ErrorBoundary>
                    <div className="kafe-layout-content">{children}</div>
                </ErrorBoundary>
            </Layout.Content>
        </>
    );
};
