import React, { useContext } from 'react';
import { API } from '../api/API';
import { User } from '../data/User';

export class Caffeine {
    public api: API;
    public user: User | null = null;

    public constructor(api: API) {
        this.api = api;

        const user = new User();
        user.email = 'rosecky.jonas@gmail.com';
        user.id = '123456789';
        user.name = 'Jonáš Rosecký';
        user.role = 'admin';

        //this.user = null;
        this.user = user;
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
