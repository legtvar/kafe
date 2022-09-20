import i18next from 'i18next';
import React from 'react';
import ReactDOM from 'react-dom/client';
import { I18nextProvider } from 'react-i18next';
import App from './App';

import './App.less';

import common_cs from './translations/cs/common.json';
import common_en from './translations/en/common.json';

i18next.init({
    interpolation: { escapeValue: false }, // React already does escaping
    lng: 'cs',
    fallbackLng: 'en',
    resources: {
        en: {
            common: common_en,
        },
        cs: {
            common: common_cs,
        },
    },
});

const root = ReactDOM.createRoot(document.getElementById('root') as HTMLElement);
root.render(
    <React.StrictMode>
        <I18nextProvider i18n={i18next}>
            <App />
        </I18nextProvider>
    </React.StrictMode>,
);
