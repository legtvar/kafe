import { components } from '../schemas/api';
import { localizedString } from '../schemas/generic';
import { getPrefered } from '../utils/preferedLanguage';
import { Artifact } from './Artifact';

export class Project {
    // API object
    public id!: string;
    public projectGroupId!: string;
    public projectGroupName!: localizedString;
    public name?: localizedString;
    public genere?: localizedString;
    public description?: localizedString;
    public visibility!: components['schemas']['Visibility'];
    public releasedOn!: Date | null;

    // Authors
    public crew!: components['schemas']['ProjectAuthorDto'][];
    public cast!: components['schemas']['ProjectAuthorDto'][];

    public artifacts!: Artifact[];

    public constructor(struct: components['schemas']['ProjectListDto'] | components['schemas']['ProjectDetailDto']) {
        Object.assign(this, struct);
        this.releasedOn = new Date(struct.releasedOn);
        if (this.releasedOn.getTime() < 0) this.releasedOn = null;
        if (this.artifacts) {
            this.artifacts = this.artifacts.map((artifact: any) => new Artifact(artifact));
        }
    }

    public getName() {
        return getPrefered(this.name);
    }

    public getDescription() {
        return getPrefered(this.description);
    }

    public getGroupName() {
        return getPrefered(this.projectGroupName);
    }

    public getGenere() {
        return getPrefered(this.genere);
    }
}
