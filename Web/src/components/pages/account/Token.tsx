import { Navigate, useParams } from 'react-router-dom';
import { AwaitAPI } from '../../utils/AwaitAPI';

export function Token() {
    const { token } = useParams();

    return (
        <AwaitAPI request={(api) => api.accounts.temporary.confirm(token || '')}>
            {(data) => {
                return <Navigate to="/auth" />;
            }}
        </AwaitAPI>
    );
}
