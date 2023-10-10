import { localizedString } from '../schemas/generic';

export function fulltextFilter(haystack: localizedString | string, needle: string) {
    if (typeof haystack === 'string') {
        return haystack.toLowerCase().includes(needle.toLowerCase());
    }

    for (const lang in haystack) {
        if (haystack.hasOwnProperty(lang)) {
            const element = haystack[lang];
            if (element.toLowerCase().includes(needle.toLowerCase())) {
                return true;
            }
        }
    }

    return false;
}
