import { components } from '../schemas/api';
import { HRIB, localizedString } from '../schemas/generic';
import { getPrefered } from '../utils/preferedLanguage';
import { AbstractType } from './AbstractType';
import { entriesMapper } from './serialize/entriesMapper';
import { localizedMapper } from './serialize/localizedMapper';
import { Serializer } from './serialize/Serializer';

export class Playlist extends AbstractType {
    // API object
    public name!: localizedString;
    public description?: localizedString;
    public entries!: {
        id: HRIB;
        name: localizedString;
    }[];

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
            .add('name', localizedMapper)
            .add('description', localizedMapper)
            .add('entries', entriesMapper)
            .build();
    }
}
