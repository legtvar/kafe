import * as React from 'react';
import { useOutlet } from 'react-router-dom';

interface IOutletOrChildrenProps {
    children: React.ReactElement | React.ReactElement[];
}

export function OutletOrChildren(props: IOutletOrChildrenProps) {
    return useOutlet() || <>{props.children}</>;
}
