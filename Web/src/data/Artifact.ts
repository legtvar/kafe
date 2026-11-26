import { components } from '../schemas/api';
import { HRIB, localizedString } from '../schemas/generic';
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
    public containingProjects!: { id: HRIB, name: localizedString }[];

    public constructor(struct: any) {
        super();
        Object.assign(this, struct);
        this.addedOn = new Date(struct.addedOn);
    }

    public getName() {
        return getPrefered(this.name);
    }
    
    public getProjectName() {
        return this.containingProjects?.length > 0 ? getPrefered(this.containingProjects[0].name) : undefined;
    }

    public getAddedOn() {
        return this.addedOn;
    }
    
    public getDisplayName() {
        const name = this.getName();
        const projectName = this.getProjectName();
        if (name === projectName || !projectName) {
            return name;
        }
        
        return `${projectName} (${name})`;
    }

    serialize(update: boolean = false): components['schemas']['ArtifactCreationDto'] {
        return new Serializer(this, update).add('id').add('blueprintSlot').build();
    }
}
