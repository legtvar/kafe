import { useParams } from 'react-router-dom';
import { useApi } from '@/hooks/Caffeine';
import { Loading } from '@/components/utils/Loading';

export function Token() {
    const { token } = useParams();
    const api = useApi();
    api.accounts.temporary.confirm(token || '');

    return <Loading center large />;
}
