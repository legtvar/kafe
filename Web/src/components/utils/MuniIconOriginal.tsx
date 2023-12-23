import { GenIcon, IconBaseProps } from 'react-icons';

export function MuniIconOriginal(props: IconBaseProps) {
    return GenIcon({
        tag: 'svg',
        attr: { viewBox: '0 0 19 19' },
        child: [
            {
                tag: 'rect',
                attr: {
                    x: '0',
                    y: '0',
                    width: '19',
                    height: '19',
                    style: {
                        fill: '#0000dc',
                    },
                },
            },
            {
                tag: 'path',
                attr: {
                    strokeWidth: '0',
                    d: 'M5.473,3.323l0,11.842l1.877,-0l0,-11.842l-1.876,0Zm1.945,0l1.172,11.842l0.639,-0l-1.173,-11.842l-0.638,0Zm3.052,0l-1.172,11.842l0.637,-0l1.172,-11.842l-0.637,0Zm0.706,0l0,11.842l1.894,-0l0,-11.842l-1.894,0Z',
                    style: {
                        fill: '#fff',
                        fillRule: 'nonzero'
                    },
                },
            } as any,
        ],
    })(props);
}
