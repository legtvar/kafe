import { components } from '../schemas/api';
import { AbstractType } from './AbstractType';

export class System extends AbstractType {
    public name!: string;
    public version!: string;
    public commit!: string;
    public commitDate!: string;
    public runningSince!: string;

    public constructor(struct: components['schemas']['EntityPermissionsDetailDto']) {
        super();
        Object.assign(this, struct);
    }
}
