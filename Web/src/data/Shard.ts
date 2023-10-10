import { components } from '../schemas/api';
import { AbstractType } from './AbstractType';

export class Shard extends AbstractType {
    // API object
    public kind!: components['schemas']['ShardKind'];
    public variants!: string[];
    // containingProjectIds: (string)[];

    public constructor(struct: any) {
        super();
        Object.assign(this, struct);
    }
}
