import { t } from 'i18next';
import { useSearchParams } from 'react-router-dom';
import { components } from '../../schemas/api';
import { useTitle } from '../../utils/useTitle';
import { Error } from '../utils/Error';

interface IServerErrorProps {}

export function ServerError(props: IServerErrorProps) {
    const [searchParams] = useSearchParams();
    const title = searchParams.get('title') ?? t('error.title');
    const error: components['schemas']['KafeProblemDetails'] = {
        title: title,
        detail: searchParams.get('detail') ?? t('error.subtitle'),
        errors: [],
    };
    useTitle(t('title.error', { error: title }));

    return <Error error={error} />;
}
