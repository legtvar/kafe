import moment from 'moment';
import { components } from '../schemas/api';
import { localizedString } from '../schemas/generic';
import { getPrefered } from '../utils/preferedLanguage';
import { AbstractType } from './AbstractType';
import { Artifact } from './Artifact';
import { localizedMapper } from './serialize/localizedMapper';
import { rolesMapper } from './serialize/rolesMapper';
import { Serializer } from './serialize/Serializer';

export class Project extends AbstractType {
    // API object
    public projectGroupId!: string;
    public projectGroupName!: localizedString;
    public name?: localizedString;
    public genre?: localizedString;
    public description?: localizedString;
    // public visibility!: components['schemas']['Visibility'];
    public releasedOn!: Date | null;

    // Authors
    public crew!: components['schemas']['ProjectAuthorDto'][];
    public cast!: components['schemas']['ProjectAuthorDto'][];

    public artifacts!: Artifact[];

    public reviews!: components['schemas']['ProjectReviewDto'][];
    public blueprint!: components['schemas']['ProjectBlueprintDto'];

    public constructor(struct: components['schemas']['ProjectListDto'] | components['schemas']['ProjectDetailDto']) {
        super();
        Object.assign(this, struct);
        this.releasedOn = new Date(struct.releasedOn);
        if (this.releasedOn.getTime() < 0) this.releasedOn = null;
        if (this.artifacts) {
            this.artifacts = this.artifacts.map((artifact: any) => new Artifact(artifact));
        }

        // Temporary
        // this.blueprint.artifactBlueprints = (
        //     this.blueprint.artifactBlueprints as any as components['schemas']['ProjectArtifactBlueprintDto'][]
        // ).reduce(
        //     (prev, curr) => ({
        //         ...prev,
        //         [(curr as any).slotName]: (
        //             curr.shardBlueprints as any as components['schemas']['ProjectArtifactShardBlueprintDto'][]
        //         ).reduce((pr, c) => ({ ...pr, [(c as any).kind]: c }), {}),
        //     }),
        //     {},
        // );
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

    public getgenre() {
        return getPrefered(this.genre);
    }

    serialize(update: boolean = false): components['schemas']['ProjectCreationDto'] {
        return new Serializer(this, update)
            .addConditionaly(!update, 'projectGroupId')
            .add('name', localizedMapper)
            .add('genre', localizedMapper)
            .add('description', localizedMapper)
            .add('visibility')
            .add('artifacts', (artifacts: Artifact[]) =>
                artifacts ? artifacts.map((artifact) => artifact.serialize(false)) : undefined,
            )
            .add('releasedOn', (date: Date | null) => moment(date).toISOString())
            .add('crew', rolesMapper)
            .add('cast', rolesMapper)
            .build();
    }
}
