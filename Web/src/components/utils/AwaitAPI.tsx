import { caffeine } from '../..';
import { Caffeine } from '../../Caffeine';
import { Await } from './Await';
import { Loading } from './Loading';

interface IAwaitAPIProps<T> {
    request: (caffeine: Caffeine) => Promise<T>;
    loader?: JSX.Element;
    children: ((data: T) => JSX.Element) | JSX.Element;
}

export function AwaitAPI<T>(props: IAwaitAPIProps<T>) {
    return (
        <caffeine.Consumer>
            {(caffeine) => (
                <Await for={props.request(caffeine)} loading={props.loader || <Loading large center />}>
                    {props.children}
                </Await>
            )}
        </caffeine.Consumer>
    );
}
