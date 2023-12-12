import { components } from '../schemas/api';
import { HRIB } from '../schemas/generic';
import { AbstractType } from './AbstractType';

export class User extends AbstractType {
    public name!: string | null;
    public emailAddress!: string;
    public preferredCulture!: string;
    public role!: 'admin' | 'temp' | 'user';
    public permissions!: Record<HRIB, Array<components["schemas"]["Permission"]>>;

    /*
        id: string;
        name?: string | null;
        uco?: string | null;
        emailAddress: string;
        preferredCulture: string;
        projects: (components["schemas"]["ProjectListDto"])[];
    */

    public constructor(struct: components['schemas']['AccountDetailDto']) {
        super();
        Object.assign(this, struct);
        this.role = this.name ? 'user' : 'temp';
        if (!this.name) this.name = this.emailAddress;
    }
}
