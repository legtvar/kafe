import { components } from '../schemas/api';
import { localizedString } from '../schemas/generic';
import { getPrefered } from '../utils/preferedLanguage';

export class Group {
    // API object
    public id!: string;
    public name!: localizedString;
    public description?: localizedString;
    public deadline?: string;
    public isOpen!: boolean;
    public validationRules?: any;

    public constructor(
        struct: components['schemas']['ProjectGroupListDto'] | components['schemas']['ProjectGroupDetailDto'],
    ) {
        Object.assign(this, struct);
    }

    public getName() {
        return getPrefered(this.name);
    }

    public getDescription() {
        return getPrefered(this.description);
    }
}
