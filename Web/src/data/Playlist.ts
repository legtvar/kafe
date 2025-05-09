import { components } from '../schemas/api';
import { HRIB, localizedString } from '../schemas/generic';
import { getPrefered } from '../utils/preferedLanguage';
import { AbstractType } from './AbstractType';
import { currentOrganizationIdMapper } from './serialize/currentOrganizationIdMapper';
import { entriesMapper } from './serialize/entriesMapper';
import { localizedMapper } from './serialize/localizedMapper';
import { Serializer } from './serialize/Serializer';

export type PlaylistEntry = {
    id: HRIB;
    name: localizedString;
};

export class Playlist extends AbstractType {
    // API object
    public name!: localizedString;
    public description?: localizedString;
    public entries!: PlaylistEntry[];
    public organizationId!: HRIB;

    public constructor(struct: components['schemas']['PlaylistListDto'] | components['schemas']['PlaylistDetailDto']) {
        super();
        Object.assign(this, struct);
    }

    public getName() {
        return getPrefered(this.name);
    }

    public getDescription() {
        return getPrefered(this.description);
    }

    serialize(update: boolean = false): components['schemas']['PlaylistCreationDto'] {
        return new Serializer(this, update)
            .addConditionaly(!update, 'organizationId', currentOrganizationIdMapper)
            .add('name', localizedMapper)
            .add('description', localizedMapper)
            .add('entries', entriesMapper, 'entryIds')
            .build();
    }
}
