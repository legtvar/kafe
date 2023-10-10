import { components } from '../schemas/api';
import { localizedString } from '../schemas/generic';
import { getPrefered } from '../utils/preferedLanguage';
import { AbstractType } from './AbstractType';
import { Project } from './Project';
import { Serializer } from './serialize/Serializer';

export class Group extends AbstractType {
    // API object
    public name!: localizedString;
    public description?: localizedString;
    public deadline?: string;
    public isOpen!: boolean;
    public projects!: Project[];
    public customFields: Record<string, any> = {};

    public constructor(
        struct: components['schemas']['ProjectGroupListDto'] | components['schemas']['ProjectGroupDetailDto'],
    ) {
        super();
        Object.assign(this, struct);
        this.projects = this.projects?.map((proj) => new Project(proj as any));
    }

    public getName() {
        return getPrefered(this.name);
    }

    public getDescription() {
        return getPrefered(this.description);
    }

    serialize(update: boolean = false): components['schemas']['GroupCreationDto'] {
        return (
            new Serializer(this, update)
                // .add('name', localizedMapper)
                // .add('genre', localizedMapper)
                // .add('description', localizedMapper)
                // .add('visibility')
                // .add('artifacts', (artifacts: Artifact[]) =>
                //     artifacts ? artifacts.map((artifact) => artifact.serialize(false)) : undefined,
                // )
                // .add('releasedOn', (date: Date | null) => moment(date).toISOString())
                // .add('crew', rolesMapper)
                // .add('cast', rolesMapper)
                .build()
        );
    }
}
