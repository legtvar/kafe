import { localizedString } from '../schemas/generic';
import { getPrefered } from '../utils/preferedLanguage';
import { Shard } from './Shard';

export class Artifact {
    // API object
    public id!: string;
    public name!: localizedString;
    public shards!: Shard[];
    public addedOn!: Date | null;
    public blueprintSlot!: string | null;
    // containingProjectIds: (string)[];

    public constructor(struct: any) {
        Object.assign(this, struct);
        this.addedOn = new Date(struct.addedOn);
    }

    public getName() {
        return getPrefered(this.name);
    }
}
