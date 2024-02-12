interface ISpaceProps {
    direction?: 'vertical' | 'horizontal';
    size: 'xs' | 's' | 'm' | 'l' | 'xl' | number | string;
}

export function Space(props: ISpaceProps) {
    const size =
        {
            xs: '0.5em',
            s: '1em',
            m: '1.5em',
            l: '2em',
            xl: '4em',
        }[props.size] || props.size;

    const horizontal = props.direction === 'horizontal';

    return (
        <div
            style={{ display: 'inline-block', width: horizontal ? size : '100%', height: !horizontal ? size : '100%' }}
        />
    );
}
