import { GenIcon, IconBaseProps } from 'react-icons';

export function MuniIcon(props: IconBaseProps) {
    return GenIcon({
        tag: 'svg',
        attr: { viewBox: '0 0 512 512' },
        child: [
            {
                tag: 'path',
                attr: {
                    strokeWidth: '0',
                    d: 'M94.717,0L174.441,0L174.441,512L94.717,512L94.717,0ZM177.367,0L204.429,0L254.179,512L227.102,512L177.367,0ZM306.84,0L333.902,0L284.166,512L257.09,512L306.84,0ZM336.828,0L417.283,0L417.283,512L336.828,512L336.828,0Z',
                },
            } as any,
        ],
    })(props);
}
