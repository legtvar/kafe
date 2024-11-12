import { Center } from '@chakra-ui/react';
import { useEffect, useState } from 'react';
import { Navigate, useMatches, useOutlet } from 'react-router-dom';
import { useApi, useAuth, useOrganizations } from '../../../hooks/Caffeine';
import { useCookieConsent } from '../../../hooks/useCookieConsent';
import { Loading } from '../../utils/Loading';
import { Status } from '../../utils/Status';

interface IRootProps {}

export function Root(props: IRootProps) {
    const { user, setUser } = useAuth();
    const { organizations, setOrganizations } = useOrganizations();
    const [status, setStatus] = useState<'ready' | 'request' | 'requesting' | 'offline'>('request');
    const outlet = useOutlet();
    const matches = useMatches();
    const api = useApi();

    useCookieConsent();

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
                        const orgDetails = await Promise.all(
                            orgs.data.map((org) => api.entities.perms.getById(org.id)),
                        );
                        orgDetails.forEach((orgDetail, i) => {
                            if (orgDetail.status === 200) {
                                orgs.data[i].globalPermissions = orgDetail.data.globalPermissions;
                                orgs.data[i].userPermissions = orgDetail.data.userPermissions;
                            }
                        });

                        setOrganizations(orgs.data);
                    }
                    setStatus('ready');
                } catch (e) {
                    setStatus('offline');
                }
            }
        })();
    });

    console.log(status);

    if (status === 'request' || status === 'requesting') {
        return (
            <Center h="100vh">
                <Loading center large />
            </Center>
        );
    }

    if (status === 'offline') {
        return <Status statusCode={'offline'} noButton />;
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
