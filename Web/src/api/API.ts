import axios from 'axios';
import { Group } from '../data/Group';
import { Playlist } from '../data/Playlist';
import { Project } from '../data/Project';

export type ApiCredentials = {
    username: string;
    password: string;
};

export class API {
    private credentials: ApiCredentials;

    public constructor(credentials: ApiCredentials) {
        this.credentials = credentials;
    }

    // API fetch functions
    public get projects() {
        const api = this;

        return {
            async getAll() {
                return api.requestArray(`https://wma.lemma.fi.muni.cz/api/v1/projects`, Project);
            },
            async getById(id: string) {
                return api.requestSingle(`https://wma.lemma.fi.muni.cz/api/v1/project/${id}`, Project);
            },
        };
    }

    public get groups() {
        const api = this;

        return {
            async getAll() {
                return api.requestArray(`https://wma.lemma.fi.muni.cz/api/v1/project-groups`, Group);
            },
            async getById(id: string) {
                return api.requestSingle(`https://wma.lemma.fi.muni.cz/api/v1/project-group/${id}`, Group);
            },
        };
    }

    public get playlists() {
        const api = this;

        return {
            async getAll() {
                return api.requestArray(`https://wma.lemma.fi.muni.cz/api/v1/playlists`, Playlist);
            },
            async getById(id: string) {
                return api.requestSingle(`https://wma.lemma.fi.muni.cz/api/v1/playlist/${id}`, Playlist);
            },
        };
    }

    // END

    private async requestSingle<Class>(path: string, type: new (response: any) => Class) {
        return this.request(path, type, false) as Promise<Class>;
    }

    private async requestArray<Class>(path: string, type: new (response: any) => Class) {
        return this.request(path, type, true) as Promise<Class[]>;
    }

    private async request<Class>(path: string, type: new (response: any) => Class, isArray: boolean) {
        // Projects
        const response = (
            await axios.get(path, {
                auth: {
                    username: this.credentials.username,
                    password: this.credentials.password,
                },
            })
        ).data;

        console.log(response);

        if (isArray) {
            return response.map((s: any) => new type(s)) as Class[];
        } else {
            return new type(response) as Class;
        }
    }
}
