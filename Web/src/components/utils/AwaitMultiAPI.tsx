import { useState } from 'react';
import { API, ApiResponse } from '../../api/API';
import { useApi } from '../../hooks/Caffeine';
import { Await } from './Await';
import { Error } from './Error';
import { Loading } from './Loading';

interface IAwaitMultiAPIProps<T> {
    request: (api: API) => Promise<ApiResponse<T>>[];
    loader?: JSX.Element;
    error?: JSX.Element | ((response: ApiResponse<T>[] | any) => JSX.Element);
    children: (data: T[]) => JSX.Element;
}

export function AwaitMultiAPI<T>(props: IAwaitMultiAPIProps<T>) {
    const api = useApi();
    const [requests] = useState<Promise<ApiResponse<T>>[]>(props.request(api));

    return (
        <Await
            for={Promise.all(requests)}
            loading={props.loader || <Loading large center />}
            error={(error) => {
                return props.error ? (
                    typeof props.error === 'function' ? (
                        props.error(error)
                    ) : (
                        props.error
                    )
                ) : (
                    <Error error={error} />
                );
            }}
            children={((data) => (
                data.every((data) => data.status === 200) ? (
                    props.children(data.map((response) => (response as any).data))
                ) : props.error ? (
                    typeof props.error === 'function' ? (
                        props.error(data)
                    ) : (
                        props.error
                    )
                ) : (
                    <Error error={(data.filter((data) => data.status !== 200)[0] as any).error} />
                )
            ))}
        />            
    );
}
