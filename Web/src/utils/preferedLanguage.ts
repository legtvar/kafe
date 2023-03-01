import i18next from 'i18next';
import { localizedString } from '../schemas/generic';

export function preferedLanguage() {
    switch (i18next.language) {
        case 'cs':
        case 'sk':
            return 'cs';
        default:
            return 'en';
    }
}

export function getPrefered(strings: localizedString) {
    const lang = preferedLanguage();

    if (!strings) return '';

    if (Object.keys(strings).includes(lang)) {
        return strings[lang];
    }
    return strings['iv'];
}
