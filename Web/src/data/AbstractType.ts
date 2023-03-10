import { HRIB } from '../schemas/generic';

export abstract class AbstractType {
    public id!: HRIB;

    public changed: Set<string | number | symbol> = new Set();

    public set<U extends keyof this>(key: U, value: this[U]) {
        this[key] = value;
        this.changed.add(key);

        return this;
    }
}
