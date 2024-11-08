import { AbstractType } from '../AbstractType';

export class Serializer<T extends AbstractType> {
    private obj: T;
    private update: boolean;
    private result: { [key: string | number | symbol]: any } = {};

    public constructor(obj: T, update: boolean) {
        this.obj = obj;
        this.update = update;
    }

    public add<U extends keyof T>(key: U, mapper: (value: T[U]) => any = (value) => value, as?: string): Serializer<T> {
        if (!this.update || this.obj.changed.has(key)) {
            this.result[as || key] = mapper(this.obj[key]);
        }
        return this;
    }

    public addAlways<U extends keyof T>(
        key: U,
        mapper: (value: T[U]) => any = (value) => value,
        as?: string,
    ): Serializer<T> {
        this.result[as || key] = mapper(this.obj[key]);
        return this;
    }

    public addConditionaly<U extends keyof T>(
        add: boolean,
        key: U,
        mapper: (value: T[U]) => any = (value) => value,
        as?: string,
    ): Serializer<T> {
        if (add) {
            this.add(key, mapper, as);
        }
        return this;
    }

    public build(): any {
        if (this.update) {
            this.result['id'] = this.obj.id;
        }
        return this.result;
    }
}
