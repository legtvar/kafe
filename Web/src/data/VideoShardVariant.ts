import { ShardVariant } from './ShardVariant';

export class VideoShardVariant extends ShardVariant {
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

    public fileExtension!: string;
    public mimeType!: string;
    public fileLength!: number;
    public duration!: string;
    public videoStreams!: {
        codec: string;
        bitrate: number;
        width: number;
        height: number;
        framerate: number;
    }[];
    public audioStreams!: {
        codec: string;
        bitrate: number;
        channels: number;
        sampleRate: number;
    }[];
    public subtitleStreams!: {
        
    }[];


    public constructor(struct: any) {
        super(struct);
    }
}
