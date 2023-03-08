import { components } from '../schemas/api';
import { localizedString } from '../schemas/generic';
import { getPrefered } from '../utils/preferedLanguage';
import { Project } from './Project';

export class Group {
    // API object
    public id!: string;
    public name!: localizedString;
    public description?: localizedString;
    public deadline?: string;
    public isOpen!: boolean;
    public projects!: Project[];

    public constructor(
        struct: components['schemas']['ProjectGroupListDto'] | components['schemas']['ProjectGroupDetailDto'],
    ) {
        Object.assign(this, struct);
        this.projects = this.projects?.map((proj) => new Project(proj as any));
    }

    public getName() {
        return getPrefered(this.name);
    }

    public getDescription() {
        return getPrefered(this.description);
    }
}
