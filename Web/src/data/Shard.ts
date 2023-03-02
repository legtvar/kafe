import { components } from '../schemas/api';

export class Shard {
    // API object
    public id!: string;
    public kind!: components['schemas']['ShardKind'];
    public variants!: string[];
    // containingProjectIds: (string)[];

    public constructor(struct: any) {
        Object.assign(this, struct);
    }
}
