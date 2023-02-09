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

    return <>Error here</>;
};
