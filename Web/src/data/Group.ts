import { components } from '../schemas/api';
import { HRIB, localizedString } from '../schemas/generic';
import { getPrefered } from '../utils/preferedLanguage';
import { AbstractType } from './AbstractType';
import { Project } from './Project';
import { Serializer } from './serialize/Serializer';
import { currentOrganizationIdMapper } from './serialize/currentOrganizationIdMapper';
import { localizedMapper } from './serialize/localizedMapper';

export class Group extends AbstractType {
    // API object
    public name!: localizedString;
    public description?: localizedString;
    public deadline?: string;
    public isOpen!: boolean;
    public projects!: Project[];
    public customFields: Record<string, any> = {};
    public organizationId!: HRIB;
    public validationSettings?: components['schemas']['ProjectValidationSettings'];

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

    serialize(update: boolean = false): components['schemas']['ProjectGroupCreationDto'] {
        return new Serializer(this, update)
            .addConditionaly(!update, 'organizationId', currentOrganizationIdMapper)
            .add('name', localizedMapper)
            .add('description', localizedMapper)
            .add('isOpen')
            .add('deadline')
            .build();
    }
}
