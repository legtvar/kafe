import { API } from '../../api/API';
import { useApi } from '../../hooks/Caffeine';
import { Await } from './Await';
import { Error } from './Error';
import { Loading } from './Loading';

interface IAwaitAPIProps<T> {
    request: (api: API) => Promise<T>;
    loader?: JSX.Element;
    error?: JSX.Element;
    children: (data: T) => JSX.Element;
}

export function AwaitAPI<T>(props: IAwaitAPIProps<T>) {
    const api = useApi();

    return (
        <Await
            for={props.request(api)}
            loading={props.loader || <Loading large center />}
            error={(error) => {
                console.log(error);
                return props.error || <Error error={error} />;
            }}
        >
            {(data) => (data ? props.children(data) : props.error || <Error error={404} />)}
        </Await>
    );
}
