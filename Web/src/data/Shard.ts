import { components } from '../schemas/api';
import { AbstractType } from './AbstractType';
import { ShardVariant } from './ShardVariant';

export class Shard extends AbstractType {
    // API object
    public kind!: components['schemas']['ShardKind'];
    public variants!: string[] | Record<string, ShardVariant>;

    public constructor(struct: any) {
        super();
        Object.assign(this, struct);
    }

    public getVariantIds(): string[] {
        if (Array.isArray(this.variants)) {
            return this.variants;
        }

        return Object.keys(this.variants);
    }
}
