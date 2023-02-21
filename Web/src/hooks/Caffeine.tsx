import React, { useContext } from 'react';
import { API } from '../api/API';
import { User } from '../data/User';

export class Caffeine {
    public api: API;
    public user: User | null = null;

    public constructor(api: API) {
        this.api = api;

        this.user = new User();
        this.user.email = 'rosecky.jonas@gmail.com';
        this.user.id = '123456789';
        this.user.name = 'Jonáš Rosecký';
        this.user.role = 'admin';
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
    };
}
