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
    public releaseDate!: Date | null;

    // Authors
    public crew!: components['schemas']['ProjectAuthorDto'][];
    public cast!: components['schemas']['ProjectAuthorDto'][];

    public artifacts!: Artifact[];

    public constructor(struct: components['schemas']['ProjectListDto'] | components['schemas']['ProjectDetailDto']) {
        Object.assign(this, struct);
        this.releaseDate = new Date(struct.releaseDate);
        if (this.releaseDate.getTime() < 0) this.releaseDate = null;
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
