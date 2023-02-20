import { components } from '../schemas/api';
import { localizedString } from '../schemas/generic';
import { getPrefered } from '../utils/preferedLanguage';

export class Playlist {
    // API object
    public id!: string;
    public name!: localizedString;
    public description?: localizedString;
    public visibility?: components['schemas']['Visibility'];

    public constructor(struct: components['schemas']['PlaylistListDto'] | components['schemas']['PlaylistDetailDto']) {
        Object.assign(this, struct);
    }

    public getName() {
        return getPrefered(this.name);
    }

    public getDescription() {
        return getPrefered(this.description);
    }
}
