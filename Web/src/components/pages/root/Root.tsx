import { Navigate, useMatches, useOutlet } from 'react-router-dom';
import { useAuth } from '../../../hooks/Caffeine';

interface IRootProps {}

export function Root(props: IRootProps) {
    const { user } = useAuth();
    const outlet = useOutlet();
    const matches = useMatches();

    let authRequested = false;
    if (matches && matches.length > 1) {
        authRequested = matches[1].pathname === '/auth';
    }

    if (authRequested && !user) {
        // User requested auth section, but is not authenticated
        return <Navigate to="/home" />;
    }
    if (!authRequested && user) {
        // User requested unauthorized section, but is authenticated
        return <Navigate to="/auth" />;
    }

    if (outlet) {
        return outlet;
    }

    if (user) {
        return <Navigate to="/auth" />;
    } else {
        return <Navigate to="/home" />;
    }
}
