import { HRIB } from '../schemas/generic';

export abstract class AbstractType {
    public id!: HRIB;

    public changed: Set<string | number | symbol> = new Set();
    private observers: Array<(newItem: AbstractType) => void> = [];

    public set<U extends keyof this>(key: U, value: this[U]) {
        this[key] = value;
        this.changed.add(key);

        this.observers.forEach((observer) => observer(this));

        return this;
    }

    public addObserver(observer: (newItem: AbstractType) => void) {
        this.observers.push(observer);
    }
}
