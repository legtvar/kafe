import axios, { AxiosProgressEvent } from 'axios';
import { Group } from '../data/Group';
import { Playlist } from '../data/Playlist';
import { Project } from '../data/Project';
import { components } from '../schemas/api';
import { localizedString } from '../schemas/generic';

export type ApiCredentials = {
    username: string;
    password: string;
};

export class API {
    private credentials: ApiCredentials;
    private apiUrl = 'https://wma.lemma.fi.muni.cz/api/v1/';

    public constructor(credentials: ApiCredentials) {
        this.credentials = credentials;
    }

    // API fetch functions
    public get projects() {
        const api = this;

        return {
            async getAll() {
                return api.requestArray(`projects`, Project);
            },
            async getById(id: string) {
                return api.requestSingle(`project/${id}`, Project);
            },
        };
    }

    public get groups() {
        const api = this;

        return {
            async getAll() {
                return api.requestArray(`project-groups`, Group);
            },
            async getById(id: string) {
                return api.requestSingle(`project-group/${id}`, Group);
            },
        };
    }

    public get playlists() {
        const api = this;

        return {
            async getAll() {
                return api.requestArray(`playlists`, Playlist);
            },
            async getById(id: string) {
                return api.requestSingle(`playlist/${id}`, Playlist);
            },
        };
    }

    public get artifacts() {
        const api = this;

        return {
            async create(name: localizedString, projectId: string) {
                return await api.post<components['schemas']['ArtifactCreationDto'], string>(`artifact`, {
                    containingProject: projectId as any,
                    name: name as any,
                });
            },
        };
    }

    public get shards() {
        const api = this;

        return {
            async create(
                artifactId: string,
                file: File,
                shardKind: components['schemas']['ShardKind'],
                onUploadProgress?: (progressEvent: AxiosProgressEvent) => void,
            ) {
                const formData = new FormData();
                formData.append('file', file, file.name);
                formData.append('shardKind', shardKind);
                formData.append('artifactId', artifactId);

                return await api.upload<FormData, string>(`shard`, formData, onUploadProgress);
            },
        };
    }

    // END

    private async requestSingle<Class>(path: string, type: new (response: any) => Class) {
        return this.get(path, type, false) as Promise<Class>;
    }

    private async requestArray<Class>(path: string, type: new (response: any) => Class) {
        return this.get(path, type, true) as Promise<Class[]>;
    }

    private async get<Class>(path: string, type: new (response: any) => Class, isArray: boolean) {
        const response = (
            await axios.get(this.apiUrl + path, {
                auth: {
                    username: this.credentials.username,
                    password: this.credentials.password,
                },
            })
        ).data;

        if (isArray) {
            return response.map((s: any) => new type(s)) as Class[];
        } else {
            return new type(response) as Class;
        }
    }

    private async post<Req, Res>(path: string, body: Req) {
        const response = (
            await axios.post(this.apiUrl + path, body, {
                auth: {
                    username: this.credentials.username,
                    password: this.credentials.password,
                },
            })
        ).data;

        return response as Res;
    }

    private async upload<Req, Res>(
        path: string,
        body: Req,
        onUploadProgress?: (progressEvent: AxiosProgressEvent) => void,
    ) {
        const response = (
            await axios.post(this.apiUrl + path, body, {
                auth: {
                    username: this.credentials.username,
                    password: this.credentials.password,
                },
                headers: {
                    'Content-Type': 'multipart/form-data',
                },
                onUploadProgress,
            })
        ).data;

        return response as Res;
    }
}
