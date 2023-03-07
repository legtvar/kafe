import axios, { Axios, AxiosProgressEvent, AxiosResponse } from 'axios';
import { Author } from '../data/Author';
import { Group } from '../data/Group';
import { Playlist } from '../data/Playlist';
import { Project } from '../data/Project';
import { User } from '../data/User';
import { components } from '../schemas/api';
import { HRIB, localizedString } from '../schemas/generic';

export type ApiCredentials = {
    username: string;
    password: string;
};

export type ApiResponse<T> =
    | {
          status: 200;
          response: AxiosResponse<any>;
          data: T;
      }
    | {
          status: 400 | 403;
          response: AxiosResponse<any>;
          error: components['schemas']['ProblemDetails'];
      };

export class API {
    private apiUrl = '/api/v1/';
    private client: Axios;

    public constructor() {
        if (window.location.hostname.startsWith('localhost') || window.location.hostname.startsWith('127.0.0.1')) {
            this.apiUrl = 'http://localhost:8000' + this.apiUrl;
        }

        this.client = axios.create({
            baseURL: this.apiUrl,
            withCredentials: true,
            validateStatus: (status) => [200, 400, 403, 404].includes(status),
        });
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
            async create(project: Project) {
                return api.post<components['schemas']['ProjectCreationDto'], HRIB>(`project`, project.serialize());
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

    public get authors() {
        const api = this;

        return {
            async getAll() {
                return api.requestArray(`authors`, Author);
            },
            async getById(id: string) {
                return api.requestSingle(`author/${id}`, Author);
            },
            async create(author: Author) {
                return api.post<components['schemas']['AuthorCreationDto'], HRIB>(`author`, author.serialize());
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
            streamUrl(id: string, variant: string) {
                return `${api.apiUrl}shard-download/${id}/${variant}`;
            },
        };
    }

    public get accounts() {
        const api = this;

        return {
            temporary: {
                async create(email: string, culture: string) {
                    return await api.post<components['schemas']['TemporaryAccountCreationDto'], {}>(
                        `tmp-account`,
                        {
                            emailAddress: email,
                            preferredCulture: culture,
                        },
                        false,
                    );
                },
                async confirm(token: string) {
                    return await api.getSimple(`tmp-account/${token}`);
                },
            },
            info: {
                async getSelf() {
                    return api.requestSingle(`account`, User);
                },
                async getById(id: string) {
                    return api.requestSingle(`account/${id}`, User);
                },
            },
            async logout() {
                // TODO: Add endpoint
                return null;
            },
        };
    }

    // END

    // Api utils

    private async requestSingle<Class>(path: string, type: new (response: any) => Class) {
        return this.get(path, type, false) as Promise<ApiResponse<Class>>;
    }

    private async requestArray<Class>(path: string, type: new (response: any) => Class) {
        return this.get(path, type, true) as Promise<ApiResponse<Class[]>>;
    }

    private async get<Class>(
        path: string,
        type: new (response: any) => Class,
        isArray: boolean,
        auth: boolean = true,
    ): Promise<ApiResponse<Class | Class[]>> {
        const response = await this.client.get(path, {
            withCredentials: auth,
        });

        const res = this.handleError<any>(response);

        if (res.status === 200) {
            if (isArray) {
                res.data = res.data.map((s: any) => new type(s)) as Class[];
            } else {
                res.data = new type(response.data) as Class;
            }
        }

        return res;
    }

    private async getSimple<Res>(path: string, auth: boolean = true): Promise<ApiResponse<any>> {
        const response = await this.client.get(path, {
            withCredentials: auth,
        });

        return this.handleError<Res>(response);
    }

    private async post<Req, Res>(path: string, body: Req, auth: boolean = true): Promise<ApiResponse<Res>> {
        const response = await this.client.post(path, body, {
            withCredentials: auth,
        });

        return this.handleError<Res>(response);
    }

    private async upload<Req, Res>(
        path: string,
        body: Req,
        onUploadProgress?: (progressEvent: AxiosProgressEvent) => void,
        auth: boolean = true,
    ): Promise<ApiResponse<Res>> {
        const response = await this.client.post(path, body, {
            withCredentials: auth,
            headers: {
                'Content-Type': 'multipart/form-data',
            },
            onUploadProgress,
        });

        return this.handleError<Res>(response);
    }

    private handleError<T>(response: AxiosResponse<any>): ApiResponse<T> {
        if (response.status === 400) {
            return {
                status: 400,
                error: response.data,
                response: response,
            };
        }

        if (response.status === 403) {
            return {
                status: 403,
                error: {
                    type: 'unauthenticated',
                    title: 'User not authenticated',
                    status: 403,
                },
                response: response,
            };
        }

        if (response.status === 403) {
            return {
                status: 403,
                error: {
                    type: 'not found',
                    title: 'File not found',
                    status: 404,
                },
                response: response,
            };
        }

        return { data: response.data as T, response: response, status: 200 };
    }
}
