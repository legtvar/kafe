import axios, { Axios, AxiosProgressEvent, AxiosResponse } from 'axios';
import { Author } from '../data/Author';
import { EntityPermissions } from '../data/EntityPermissions';
import { Group } from '../data/Group';
import { Playlist } from '../data/Playlist';
import { Project } from '../data/Project';
import { User } from '../data/User';
import { components } from '../schemas/api';
import { HRIB, localizedString } from '../schemas/generic';
import { IntRange } from '../utils/IntRange';

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
          status: IntRange<400, 500>;
          response: AxiosResponse<any>;
          error: components['schemas']['ProblemDetails'];
      };

export class API {
    private apiUrl = '/api/v1/';
    private client: Axios;
    private static Production = "https://kafe.fi.muni.cz";
    private static Staging = "https://kafe-stage.fi.muni.cz";

    public constructor() {
        if (window.location.hostname.startsWith('localhost') || window.location.hostname.startsWith('127.0.0.1')) {
            this.apiUrl = 'https://localhost:44369' + this.apiUrl;
        } else {
            this.apiUrl = window.location.origin + this.apiUrl;
        }

        this.client = axios.create({
            baseURL: this.apiUrl,
            withCredentials: true,
            validateStatus: (status) => [200].includes(status) || (status >= 400 && status < 500),
        });
    }
    
    public get isProduction() {
        return new URL(this.apiUrl).origin === API.Production;
    }

    public get isStaging() {
        return new URL(this.apiUrl).origin === API.Staging;
    }
    
    public get isLocalhost() {
        const url = new URL(this.apiUrl);
        return url.hostname === "localhost" || url.hostname === "127.0.0.1";
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
            async validationById(id: string) {
                return api.getSimple<components['schemas']['ProjectValidationDto']>(`project-validation/${id}`);
            },
            async create(project: Project) {
                return api.post<components['schemas']['ProjectCreationDto'], HRIB>(`project`, project.serialize());
            },
            async update(project: Project) {
                return api.patch<components['schemas']['ProjectCreationDto'], HRIB>(`project`, project.serialize(true));
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
            async create(group: Group) {
                return api.post<components['schemas']['ProjectGroupCreationDto'], HRIB>(`group`, group.serialize());
            },
            async update(group: Group) {
                return api.patch<components['schemas']['ProjectGroupCreationDto'], HRIB>(
                    `group`,
                    group.serialize(true),
                );
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
            async create(name: localizedString, projectId: string, blueprintSlot: string) {
                return await api.post<components['schemas']['ArtifactCreationDto'], string>(`artifact`, {
                    containingProject: projectId as any,
                    name: name as any,
                    blueprintSlot,
                });
            },
        };
    }

    public get review() {
        const api = this;

        return {
            async create(
                projectId: HRIB,
                kind: components['schemas']['ReviewKind'],
                reviewerRole: string,
                comment: string,
            ) {
                return await api.post<any, HRIB>(`project-review`, {
                    projectId,
                    kind,
                    reviewerRole,
                    comment: {
                        iv: comment,
                    },
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
                formData.append('kind', shardKind);
                formData.append('artifactId', artifactId);

                return await api.upload<FormData, string>(`shard`, formData, onUploadProgress);
            },
            streamUrl(id: string, variant: string) {
                return `${api.apiUrl}shard-download/${id}/${variant}`;
            },
            defaultStreamUrl(id: string) {
                return `${api.apiUrl}shard-download/${id}`;
            },
        };
    }

    public get accounts() {
        const api = this;

        return {
            temporary: {
                async create(email: string, culture: string) {
                    return await api.post<components['schemas']['TemporaryAccountCreationDto'], Record<string, never>>(
                        `tmp-account`,
                        {
                            emailAddress: email,
                            preferredCulture: culture,
                        },
                        false,
                    );
                },
                async confirm(token: string) {
                    return api.getSimple(`tmp-account/${token}`);
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
            external: {
                loginUrl() {
                    return `${api.apiUrl}account/external-login?redirect=${window.location.origin}/auth`;
                },
            },
            async logout() {
                return api.getSimple(`account/logout`);
            },
        };
    }

    public get entities() {
        const api = this;

        return {
            perms: {
                async getById(id: string) {
                    return await api.requestSingle(`entity/perms/${id}`, EntityPermissions);
                },
                async update(perms: EntityPermissions) {
                    return await api.patch<components['schemas']['EntityPermissionsAccountEditDto'], HRIB>(
                        `entity/perms`,
                        perms.serialize(true),
                    );
                },
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

    private async patch<Req, Res>(path: string, body: Req, auth: boolean = true): Promise<ApiResponse<Res>> {
        const response = await this.client.patch(path, body, {
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
        if (response.status !== 200) {
            return {
                status: response.status as any,
                error: response.data,
                response: response,
            };
        }
        return { data: response.data as T, response: response, status: 200 };
    }
}
