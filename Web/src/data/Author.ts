import { components } from '../schemas/api';
import { localizedString } from '../schemas/generic';
import { getPrefered } from '../utils/preferedLanguage';
import { AbstractType } from './AbstractType';
import { Serializer } from './serialize/Serializer';

export class Author extends AbstractType {
    public name!: string;
    public bio!: localizedString;
    public email!: string;
    public phone!: string;
    public uco!: string;

    public getBio(): string {
        return getPrefered(this.bio);
    }

    public constructor(struct: components['schemas']['AuthorListDto'] | components['schemas']['AuthorDetailDto']) {
        super();
        Object.assign(this, struct);
    }

    serialize(changesOnly: boolean = false): components['schemas']['AuthorCreationDto'] {
        return new Serializer(this, changesOnly).add('name').add('bio').add('uco').add('email').add('phone').build();
    }
}
