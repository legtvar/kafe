interface ILoadingProps {
    center?: true;
    large?: true;
}

export function Loading(props: ILoadingProps) {
    const spinner = <>Loading...</>;

    if (props.center) {
        return <div style={{ minHeight: 200, justifyContent: 'center' }}>{spinner}</div>;
    }
    return spinner;
}
