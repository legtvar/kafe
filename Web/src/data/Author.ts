import { components } from '../schemas/api';
import { localizedString } from '../schemas/generic';
import { AbstractType } from './AbstractType';
import { Serializer } from './Serializer';

export class Author extends AbstractType {
    public name!: string;
    public bio!: localizedString;
    public email!: string;
    public phone!: string;
    public uco!: string;
    public visibility!: components['schemas']['Visibility'];

    /*
        "id": "string",
        "name": "string",
        "visibility": "Unknown",
        "bio": {
            "iv": "string",
            "cs": "string",
            "en": "string"
        },
        "uco": "string",
        "email": "string",
        "phone": "string"
    */

    public constructor(struct: components['schemas']['AuthorListDto'] | components['schemas']['AuthorDetailDto']) {
        super();
        Object.assign(this, struct);
    }

    serialize(changesOnly: boolean = false): components['schemas']['AuthorCreationDto'] {
        return new Serializer(this, changesOnly)
            .add('name')
            .add('visibility')
            .add('bio')
            .add('uco')
            .add('email')
            .add('phone')
            .build();
    }
}
