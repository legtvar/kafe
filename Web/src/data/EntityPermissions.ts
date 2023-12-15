import { components } from '../schemas/api';
import { HRIB } from '../schemas/generic';
import { AbstractType } from './AbstractType';

export class EntityPermissions extends AbstractType {
    public globalPermissions!: Array<components["schemas"]["Permission"]> | null;
    public userPermissions!: Array<components["schemas"]["Permission"]> | null;
    public accountPermissions!: Record<HRIB, Array<components["schemas"]["Permission"]>>;

    public constructor(struct: components['schemas']['EntityPermissionsDetailDto']) {
        super();
        Object.assign(this, struct);
    }
}
