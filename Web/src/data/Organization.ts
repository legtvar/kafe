import { components } from '../schemas/api';
import { localizedString, Permission } from '../schemas/generic';
import { getPrefered } from '../utils/preferedLanguage';
import { AbstractType } from './AbstractType';
import { EntityPermissionsUser } from './EntityPermissions';
import { localizedMapper } from './serialize/localizedMapper';
import { Serializer } from './serialize/Serializer';

export class Organization extends AbstractType {
    public name!: localizedString;
    public createdOn: Date | null = null;
    public globalPermissions!: Array<Permission> | null;
    public userPermissions!: Array<Permission> | null;
    public accountPermissions!: Array<EntityPermissionsUser>;

    public constructor(
        struct: components['schemas']['OrganizationListDto'] | components['schemas']['OrganizationDetailDto'],
    ) {
        super();
        Object.assign(this, struct);
        7;
        if ('createdOn' in struct && struct.createdOn) {
            this.createdOn = new Date(struct.createdOn);
        }
    }

    public getName() {
        return getPrefered(this.name);
    }

    serialize(update: boolean = false): components['schemas']['ProjectCreationDto'] {
        return new Serializer(this, update).add('name', localizedMapper).build();
    }
}
