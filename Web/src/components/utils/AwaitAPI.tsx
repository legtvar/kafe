import { API } from '../../api/API';
import { useApi } from '../../hooks/Caffeine';
import { Await } from './Await';
import { Error } from './Error';
import { Loading } from './Loading';

interface IAwaitAPIProps<T> {
    request: (api: API) => Promise<T>;
    loader?: JSX.Element;
    children: ((data: T) => JSX.Element) | JSX.Element;
}

export function AwaitAPI<T>(props: IAwaitAPIProps<T>) {
    const api = useApi();

    return (
        <Await
            for={props.request(api)}
            loading={props.loader || <Loading large center />}
            error={(error) => <Error error={error} />}
        >
            {props.children}
        </Await>
    );
}
