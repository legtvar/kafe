import { Center, Spinner } from '@chakra-ui/react';

interface ILoadingProps {
    center?: true;
    large?: true;
}

export function Loading(props: ILoadingProps) {
    const spinner = <Spinner size="xl" />;

    if (props.center) {
        return <Center my={16}>{spinner}</Center>;
    }
    return spinner;
}
