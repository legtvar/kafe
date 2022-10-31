import { LoadingOutlined } from '@ant-design/icons';
import { Layout, Spin } from 'antd';

interface ILoadingProps {
    center?: true;
    large?: true;
}

export function Loading(props: ILoadingProps) {
    const antIcon = <LoadingOutlined style={{ fontSize: props.large ? 80 : 24 }} spin />;

    const spinner = <Spin indicator={antIcon} />;

    if (props.center) {
        return <Layout style={{ minHeight: 200, justifyContent: 'center' }}>{spinner}</Layout>;
    }
    return spinner;
}
