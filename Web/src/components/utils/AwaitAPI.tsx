import { useConst } from '@chakra-ui/react';
import { API, ApiResponse } from '../../api/API';
import { useApi } from '../../hooks/Caffeine';
import { Await } from './Await';
import { Error } from './Error';
import { Loading } from './Loading';

interface IAwaitAPIProps<T> {
    request: (api: API) => Promise<ApiResponse<T>>;
    loader?: JSX.Element;
    error?: JSX.Element;
    children: (data: T) => JSX.Element;
}

export function AwaitAPI<T>(props: IAwaitAPIProps<T>) {
    const api = useApi();
    const request = useConst(props.request(api));

    return (
        <Await
            for={request}
            loading={props.loader || <Loading large center />}
            error={(error) => {
                return props.error || <Error error={error} />;
            }}
        >
            {(data) => (data.status === 200 ? props.children(data.data) : props.error || <Error error={data.error} />)}
        </Await>
    );
}
