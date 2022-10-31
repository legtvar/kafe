import { API } from './api/API';

export class Caffeine {
    public api: API;

    public constructor(api: API) {
        this.api = api;
    }
}
