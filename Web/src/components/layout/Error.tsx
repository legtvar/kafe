import { Button, Result } from 'antd';
import { t } from 'i18next';
import React from 'react';
import { useLinkClickHandler, useRouteError } from 'react-router-dom';

export interface IErrorProps {
    error?:
        | string
        | {
              status: string | number;
              statusText: string;
              message?: string;
          };
}

export const Error: React.FC<IErrorProps> = (props: IErrorProps) => {
    let error = useRouteError() as any;
    const backlink = useLinkClickHandler('/');

    if (props.error) {
        error = {
            status: 'error',
            statusText: props.error,
        };
    }

    console.error(error);

    return (
        <Result
            status={error.status}
            title={error.statusText}
            subTitle={error.message}
            extra={
                <Button type="primary" onClick={backlink}>
                    {t('common.backHome').toString()}
                </Button>
            }
        />
    );
};
