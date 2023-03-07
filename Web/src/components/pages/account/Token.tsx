import { t } from 'i18next';
import { Navigate, useParams } from 'react-router-dom';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { Error } from '../../utils/Error';
import { Status } from '../../utils/Status';

export function Token() {
    const { token } = useParams();

    return (
        <AwaitAPI
            request={(api) => api.accounts.temporary.confirm(token || '')}
            error={(response) =>
                response.status === 403 ? (
                    <Status statusCode={t(`error.tokenInvalid`).toString()} />
                ) : (
                    <Error error={response.error} />
                )
            }
        >
            {(data) => {
                return <Navigate to="/auth" />;
            }}
        </AwaitAPI>
    );
}
