import { components } from '../schemas/api';
import { HRIB, Permission } from '../schemas/generic';
import { AbstractType } from './AbstractType';
import { User } from './User';
import { Serializer } from './serialize/Serializer';

export type EntityPermissionsUser = components["schemas"]["EntityPermissionsAccountListDto"]
    & { permissions: Array<Permission> };

export class EntityPermissions extends AbstractType {
    public globalPermissions!: Array<Permission> | null;
    public userPermissions!: Array<Permission> | null;
    public accountPermissions!: Array<EntityPermissionsUser>;

    public constructor(struct: components['schemas']['EntityPermissionsDetailDto']) {
        super();
        Object.assign(this, struct);
    }
    
    serialize(update: boolean = false): components['schemas']['EntityPermissionsAccountEditDto'] {
        return new Serializer(this, update)
            .add('globalPermissions')
            .add('accountPermissions')
            .build();
    }
}
