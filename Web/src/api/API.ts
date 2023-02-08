import axios from 'axios';
import { forValueDefined } from 'waitasecond';
import { Group } from '../data/Group';
import { Project } from '../data/Project';
import { components } from '../schemas/api';
import { storageOrPrompt } from '../utils/storageOrPrompt';

function getById<T>(data: Array<T extends { id: string } ? T : never>, id: string) {
    const found = data.filter((item) => item.id === id);
    if (found && found.length > 0) {
        return found[0];
    }
    return null;
}

export class API {
    public constructor() {
        this.fetchAll();
    }

    // API fetch functions
    public get projects() {
        const api = this;

        return {
            async getAll() {
                return await forValueDefined(() => api.data.projects);
            },
            async getById(id: string) {
                return getById(await forValueDefined(() => api.data.projects), id);
            },
        };
    }

    public get groups() {
        const api = this;

        return {
            async getAll() {
                return await forValueDefined(() => api.data.groups);
            },
            async getById(id: string) {
                return getById(await forValueDefined(() => api.data.groups), id);
            },
        };
    }

    // END

    // === To be changed (does not get refetched on change...) ===
    private data = {
        projects: null as Project[] | null,
        groups: null as Group[] | null,
    };

    private async fetchAll() {
        // Get credentials from the local storage or request them from the user
        const username = (await storageOrPrompt('dev_username')) as string;
        const password = (await storageOrPrompt('dev_password')) as string;

        try {
            // Projects
            const projects = (
                await axios.get('https://wma.lemma.fi.muni.cz/api/v1/projects', {
                    auth: {
                        username,
                        password,
                    },
                })
            ).data as components['schemas']['ProjectListDto'][];

            this.data.projects = projects.map((s) => new Project(s));

            // Project groups
            const groups = (
                await axios.get('https://wma.lemma.fi.muni.cz/api/v1/project-groups', {
                    auth: {
                        username,
                        password,
                    },
                })
            ).data as components['schemas']['ProjectGroupListDto'][];

            this.data.groups = groups.map((s) => new Group(s));

            console.log(this.data);
        } catch (e) {
            console.error('Error fetching data from the API', e);

            this.data.projects = [];
            this.data.groups = [];
        }
    }
}
