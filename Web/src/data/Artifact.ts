import { localizedString } from '../schemas/generic';
import { getPrefered } from '../utils/preferedLanguage';
import { Shard } from './Shard';

export class Artifact {
    // API object
    public id!: string;
    public name!: localizedString;
    public shards!: Shard[];
    // containingProjectIds: (string)[];

    public constructor(struct: any) {
        Object.assign(this, struct);
    }

    public getName() {
        return getPrefered(this.name);
    }
}
