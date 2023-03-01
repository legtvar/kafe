import { InitOptions } from "i18next";

import common_cs from './translations/cs/common.json';
import common_en from './translations/en/common.json';

export const languageConfig: InitOptions = {
    interpolation: { escapeValue: false }, // React already does escaping
    lng: 'cs',
    fallbackLng: 'en',
    defaultNS: 'common',
    resources: {
        en: {
            common: common_en,
        },
        cs: {
            common: common_cs,
        },
    },
}