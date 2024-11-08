import React, { useContext } from 'react';
import { useLocation } from 'react-router-dom';
import { API } from '../api/API';
import { Organization } from '../data/Organization';
import { User } from '../data/User';

export class Caffeine {
    public api: API;
    public user: User | null = null;
    public organizations: Organization[] = [];

    public constructor(api: API) {
        this.api = api;
        this.user = null;
        this.organizations = [];
    }
}

const caffeineContext = React.createContext<Caffeine>(undefined!);
export const CaffeineProvider = caffeineContext.Provider;

export function useApi() {
    const caffeine = useContext(caffeineContext);

    return caffeine.api;
}

export function useAuth() {
    const caffeine = useContext(caffeineContext);

    return {
        user: caffeine.user,
        setUser: (user: User | null) => {
            caffeine.user = user;
        },
    };
}

export function useOrganizations() {
    const caffeine = useContext(caffeineContext);

    const location = useLocation();
    const orgId = location.pathname.split('/')[2];

    return {
        organizations: caffeine.organizations,
        setOrganizations: (organizations: Organization[]) => {
            caffeine.organizations = organizations;
        },
        currentOrganization: caffeine.organizations.find((org) => org.id === orgId),
    };
}
