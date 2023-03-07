import moment from 'moment';
import { components } from '../schemas/api';
import { localizedString } from '../schemas/generic';
import { getPrefered } from '../utils/preferedLanguage';
import { AbstractType } from './AbstractType';
import { Artifact } from './Artifact';
import { Serializer } from './Serializer';

export class Project extends AbstractType {
    // API object
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
        super();
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

    serialize(update: boolean = false): components['schemas']['ProjectCreationDto'] {
        const rolesMapper = (people: components['schemas']['ProjectAuthorDto'][]) =>
            (people || []).map((person) => ({
                id: person.id,
                roles: person.roles,
            }));

        return new Serializer(this, update)
            .addConditionaly(!update, 'projectGroupId')
            .add('name')
            .add('genere')
            .add('description')
            .add('visibility')
            .add('releasedOn', (date: Date | null) => moment(date).toISOString())
            .add('crew', rolesMapper)
            .add('cast', rolesMapper)
            .build();
    }
}
