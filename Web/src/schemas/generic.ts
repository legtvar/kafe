import { components } from './api';

export type localizedString =
    | {
          [key: string | 'iv']: string;
      }
    | undefined;

export function toLocalizedString(input: any): localizedString {
    return input;
}

export const concat = (...parts: (localizedString | string)[]) => {
    let result = {
        iv: '',
        cs: '',
        en: '',
    };

    for (let i = 0; i < parts.length; i++) {
        let part = parts[i];
        if (typeof part === 'string') {
            part = {
                iv: part,
                en: part,
                cs: part,
            };
        }
        result.iv += part!.iv;
        result.cs += part!.cs || part!.cs;
        result.en += part!.en || part!.en;
    }

    return result as localizedString;
};

export type ArtifactFootprint = {
    id: string;
    name: localizedString;
    shards: {
        id: string;
        kind: components['schemas']['ShardKind'];
    }[];
};

export type HRIB = string;

// NB: Keep in sync with Common/Hrib.cs
export const SystemHRIB = 'system';
