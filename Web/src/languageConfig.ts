import { InitOptions } from 'i18next';

import common_cs from './translations/cs/common.json';
import fffimu24_details_cs from './translations/cs/fffimu24-details.i18n.html';
import fffimu24_general_cs from './translations/cs/fffimu24-general.i18n.html';
import common_en from './translations/en/common.json';
import fffimu24_details_en from './translations/en/fffimu24-details.i18n.html';
import fffimu24_general_en from './translations/en/fffimu24-general.i18n.html';

export const LS_LANGUAGE_APP_KEY = 'kafe_language_app';
const ls_lang = localStorage.getItem(LS_LANGUAGE_APP_KEY) || 'en';

export const languageConfig: InitOptions = {
    interpolation: { escapeValue: false }, // React already does escaping
    lng: ls_lang,
    fallbackLng: 'en',
    defaultNS: 'common',
    resources: {
        en: {
            common: {
                ...common_en,
                homeFestival: {
                    title: common_en.homeFestival.title,
                    general: fffimu24_general_en,
                    details: fffimu24_details_en,
                },
            },
        },
        cs: {
            common: {
                ...common_cs,
                homeFestival: {
                    title: common_cs.homeFestival.title,
                    general: fffimu24_general_cs,
                    details: fffimu24_details_cs,
                },
            },
        },
    },
};
