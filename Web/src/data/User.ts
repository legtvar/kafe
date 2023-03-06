import { components } from '../schemas/api';
import { HRIB } from '../schemas/generic';
import { Project } from './Project';

export class User {
    public id!: HRIB;
    public name!: string | null;
    public emailAddress!: string;
    public preferredCulture!: string;
    public projects!: Project[];
    public role!: 'admin' | 'temp' | 'user';

    /*
        id: string;
        name?: string | null;
        uco?: string | null;
        emailAddress: string;
        preferredCulture: string;
        projects: (components["schemas"]["ProjectListDto"])[];
    */

    public constructor(struct: components['schemas']['AccountDetailDto']) {
        Object.assign(this, struct);
        this.projects = (this.projects as any[]).map((project) => new Project(project));
        this.role = this.name ? 'user' : 'temp';
        if (!this.name) this.name = this.emailAddress;
    }
}
