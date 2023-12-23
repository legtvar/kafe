import { useSearchParams } from 'react-router-dom';
import { Error } from '../utils/Error';
import { t } from 'i18next';

interface IServerErrorProps {}

export function ServerError(props: IServerErrorProps) {
    const [searchParams] = useSearchParams();

    const error = {
        title: searchParams.get('title') ?? t('error.title'),
        detail: searchParams.get('detail') ?? t('error.subtitle'),
    };

    return <Error error={error} />;
}
