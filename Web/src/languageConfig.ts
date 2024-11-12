import { InitOptions } from 'i18next';
import { resources } from './translations/resources';

export const LS_LANGUAGE_APP_KEY = 'kafe_language_app';

let preferredLanguage = navigator.language.split('-')[0];
if (!Object.keys(resources).includes(preferredLanguage)) {
    preferredLanguage = 'en';
}

const ls_lang = localStorage.getItem(LS_LANGUAGE_APP_KEY) || preferredLanguage;

export const languageConfig: InitOptions = {
    interpolation: { escapeValue: false }, // React already does escaping
    lng: ls_lang,
    fallbackLng: 'en',
    defaultNS: 'common',
    resources,
};
