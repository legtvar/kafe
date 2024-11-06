import { AbstractType } from './AbstractType';

export class ShardVariant extends AbstractType {
    // API object
    
    // "fileExtension": ".webm",
    // "mimeType": "video/webm",
    // "fileLength": 21296851,
    // "duration": "00:06:38.0010000",
    // "videoStreams": [
    // {
    //     "codec": "vp9",
    //     "bitrate": 0,
    //     "width": 854,
    //     "height": 480,
    //     "framerate": 24
    // }
    // ],
    // "audioStreams": [
    // {
    //     "codec": "opus",
    //     "bitrate": 0,
    //     "channels": 2,
    //     "sampleRate": 48000
    // }
    // ],
    // "subtitleStreams": [],

    public isCorrupted!: boolean;
    public error!: any | null;

    public constructor(struct: any) {
        super();
        Object.assign(this, struct);
    }
}
