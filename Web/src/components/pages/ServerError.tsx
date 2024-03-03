import { useSearchParams } from 'react-router-dom';
import { Error } from '../utils/Error';
import { t } from 'i18next';
import { useTitle } from '../../utils/useTitle';

interface IServerErrorProps {}

export function ServerError(props: IServerErrorProps) {
    const [searchParams] = useSearchParams();
    const title = searchParams.get('title') ?? t('error.title')
    const error = {
        title,
        detail: searchParams.get('detail') ?? t('error.subtitle'),
    };
    useTitle(t("title.error", {error: title}));

    return <Error error={error} />;
}
