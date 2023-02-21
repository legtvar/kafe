import React from 'react';

export function useReload() {
    const [rerender, setRerender] = React.useState(false);
    return () => setRerender(!rerender);
}

export function useReloadVar() {
    const [rerender, setRerender] = React.useState(false);
    return {
        reload: () => setRerender(!rerender),
        value: rerender,
    };
}
