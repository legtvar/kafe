import React, { useContext } from 'react';
import { API } from '../api/API';
import { User } from '../data/User';

export class Caffeine {
    public api: API;
    public user: User | null = null;

    public constructor(api: API) {
        this.api = api;
        this.user = null;
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
