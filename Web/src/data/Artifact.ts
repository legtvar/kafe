import { components } from '../schemas/api';
import { localizedString } from '../schemas/generic';
import { getPrefered } from '../utils/preferedLanguage';
import { AbstractType } from './AbstractType';
import { Serializer } from './serialize/Serializer';
import { Shard } from './Shard';

export class Artifact extends AbstractType {
    // API object
    public name!: localizedString;
    public shards!: Shard[];
    public addedOn!: Date | null;
    public blueprintSlot!: string | null;
    public containingProjectIds!: string[];

    public constructor(struct: any) {
        super();
        Object.assign(this, struct);
        this.addedOn = new Date(struct.addedOn);
    }

    public getName() {
        return getPrefered(this.name);
    }

    public getAddedOn() {
        return this.addedOn;
    }

    serialize(update: boolean = false): components['schemas']['ArtifactCreationDto'] {
        return new Serializer(this, update).add('id').add('blueprintSlot').build();
    }
}
