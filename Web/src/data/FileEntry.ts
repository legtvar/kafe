import mime from 'mime';
import { localizedString } from '../schemas/generic';
import { getPrefered } from '../utils/preferedLanguage';

export class FileEntry {
    // API object
    public id!: string;
    public name!: localizedString;
    public path!: string;

    public constructor(struct: any) {
        Object.assign(this, struct);
    }

    public getMime() {
        return mime.getType(this.path);
    }

    public getName() {
        return getPrefered(this.name);
    }
}
