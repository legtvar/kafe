import { Rights } from '../components/utils/RightsItem';
import { components } from '../schemas/api';
import { HRIB } from '../schemas/generic';
import { AbstractType } from './AbstractType';
import { User } from './User';
import { Serializer } from './serialize/Serializer';

export type EntityPermissionsUser = components["schemas"]["EntityPermissionsAccountListDto"]
    & { permissions: Array<Rights> };

export class EntityPermissions extends AbstractType {
    public globalPermissions!: Array<Rights> | null;
    public userPermissions!: Array<Rights> | null;
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
