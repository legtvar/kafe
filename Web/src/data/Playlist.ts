import { components } from '../schemas/api';
import { localizedString } from '../schemas/generic';
import { getPrefered } from '../utils/preferedLanguage';
import { AbstractType } from './AbstractType';

export class Playlist extends AbstractType {
    // API object
    public name!: localizedString;
    public description?: localizedString;
    public visibility?: components['schemas']['Visibility'];
    public videos!: string[];
    public customFields: Record<string, any> = {};

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
}
