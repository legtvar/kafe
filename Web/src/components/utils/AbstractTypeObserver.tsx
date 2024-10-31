import { useForceUpdate } from '@chakra-ui/react';
import { useEffect } from 'react';
import { AbstractType } from '../../data/AbstractType';

export interface IAbstractTypeObserverProps<T extends AbstractType> {
    item: T;
    children: (item: T) => React.ReactNode;
}

export function AbstractTypeObserver<T extends AbstractType>({ item, children }: IAbstractTypeObserverProps<T>) {
    const fu = useForceUpdate();

    useEffect(() => {
        item.addObserver((newItem: AbstractType) => {
            fu();
        });
    }, [item]);

    return children(item);
}

export function observeAbstactType<T extends AbstractType>(children: (item: T) => React.ReactNode) {
    return (item: T) => {
        return <AbstractTypeObserver<T> item={item}>{(item) => children(item)}</AbstractTypeObserver>;
    };
}
