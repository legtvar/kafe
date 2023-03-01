import { components } from '../schemas/api';
import { localizedString } from '../schemas/generic';
import { getPrefered } from '../utils/preferedLanguage';
import { FileEntry } from './FileEntry';

export class Project {
    // API object
    public id!: string;
    public projectGroupId!: string;
    public projectGroupName!: localizedString;
    public name?: localizedString;
    public genere?: localizedString;
    public description?: localizedString;
    public visibility!: components['schemas']['Visibility'];
    public releaseDate!: Date | null;

    // Authors
    public crew!: components['schemas']['ProjectAuthorDto'][];
    public cast!: components['schemas']['ProjectAuthorDto'][];

    public artifacts!: components['schemas']['ArtifactDetailDto'][];

    public constructor(struct: components['schemas']['ProjectListDto'] | components['schemas']['ProjectDetailDto']) {
        Object.assign(this, struct);
        this.releaseDate = new Date(struct.releaseDate);
        if (this.releaseDate.getTime() < 0) this.releaseDate = null;
    }

    public getName() {
        return getPrefered(this.name);
    }

    public getDescription() {
        return getPrefered(this.description);
    }

    public getGroupName() {
        return getPrefered(this.projectGroupName);
    }

    public getFiles() {
        return [
            new FileEntry({
                id: '1337_video1',
                name: { iv: 'Trailer' },
                path: 'https://samplelib.com/lib/preview/mp4/sample-5s.mp4',
            }),
            new FileEntry({
                id: '1337_video2',
                name: { iv: 'Film' },
                path: 'https://samplelib.com/lib/preview/mp4/sample-20s.mp4',
            }),
            new FileEntry({
                id: '1337_poster',
                name: { iv: 'Plakát' },
                path: 'https://api.lorem.space/image/movie?w=300&h=400&.png',
            }),
            new FileEntry({
                id: '1337_crew',
                name: { iv: 'Crew' },
                path: 'https://api.lorem.space/image/face?w=600&h=400&.png',
            }),
        ];
    }
}
