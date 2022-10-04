import { Button, Layout, Result } from 'antd';
import React from 'react';
import { useLinkClickHandler, useRouteError } from 'react-router-dom';

export const Error: React.FC = () => {
    const error = useRouteError() as any;
    const backlink = useLinkClickHandler("/");
    console.error(error);
    
    return (
        <Layout style={{ minHeight: '100vh', justifyContent: "center" }}>
            <Result
                status={error.status}
                title={error.statusText}
                subTitle={error.message}
                extra={<Button type="primary" onClick={backlink}>Back Home</Button>}
            />
        </Layout>
    );
};
