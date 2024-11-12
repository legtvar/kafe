import i18next from 'i18next';
import { localizedString } from '../schemas/generic';

export const LS_LANGUAGE_CONTENT_KEY = 'kafe_language_content';
let _preferedLanguage = localStorage.getItem(LS_LANGUAGE_CONTENT_KEY) || '_inherit';

export function getRawPrefered() {
    return _preferedLanguage;
}

export function preferedLanguage() {
    if (_preferedLanguage !== '_inherit') {
        return _preferedLanguage;
    }

    switch (i18next.language) {
        case 'cs':
        case 'sk':
            return 'cs';
        default:
            return 'en';
    }
}

export function getPrefered(strings: localizedString, lang?: string) {
    lang = lang || preferedLanguage();

    if (!strings) return '';

    if (Object.keys(strings).includes(lang)) {
        return strings[lang];
    }
    return strings['iv'];
}

export function setPreferedLanguage(lang: string) {
    _preferedLanguage = lang;
}
