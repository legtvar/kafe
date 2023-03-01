import { components } from './api';

export type localizedString =
    | {
          [key: string | 'iv']: string;
      }
    | undefined;

export type ArtifactFootprint = {
    id: string;
    name: localizedString;
    shards: {
        id: string;
        kind: components['schemas']['ShardKind'];
    }[];
};

export type HRIB = string;
