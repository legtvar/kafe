import { t } from 'i18next';
import { useCallback } from 'react';
import { useParams } from 'react-router-dom';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { NavigateRestoreBacklink } from '../../utils/Backlink/Backlink';
import { Error } from '../../utils/Error';
import { Status } from '../../utils/Status';

export function Token() {
    const { token } = useParams();

    return (
        <AwaitAPI
            request={useCallback((api) => api.accounts.temporary.confirm(token || ''), [token])}
            error={(response) =>
                response.status === 403 ? (
                    <Status statusCode={t(`error.tokenInvalid`).toString()} />
                ) : (
                    <Error error={response.error} />
                )
            }
        >
            {(data) => {
                return <NavigateRestoreBacklink to="/auth" />;
            }}
        </AwaitAPI>
    );
}
