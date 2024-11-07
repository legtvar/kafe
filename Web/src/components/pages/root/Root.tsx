import { useEffect, useState } from 'react';
import { Navigate, useMatches, useOutlet } from 'react-router-dom';
import { useApi, useAuth, useOrganizations } from '../../../hooks/Caffeine';
import { Loading } from '../../utils/Loading';

interface IRootProps {}

export function Root(props: IRootProps) {
    const { user, setUser } = useAuth();
    const { organizations, setOrganizations } = useOrganizations();
    const [status, setStatus] = useState<'ready' | 'request' | 'requesting'>('request');
    const outlet = useOutlet();
    const matches = useMatches();
    const api = useApi();

    useEffect(() => {
        (async () => {
            if (status === 'request') {
                setStatus('requesting');
                try {
                    const self = await api.accounts.info.getSelf();
                    if (self.status === 200) {
                        setUser(self.data);
                    }

                    const orgs = await api.organizations.getAll();
                    if (orgs.status === 200) {
                        setOrganizations(orgs.data);
                    }
                } finally {
                    setStatus('ready');
                }
            }
        })();
    });

    if (status !== 'ready') {
        return <Loading center large />;
    }

    // console.log(user);

    let authRequested = false;
    if (matches && matches.length > 1) {
        authRequested = matches[1].pathname === '/auth';

        if (matches[1].pathname === '/go') {
            return outlet;
        }
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
