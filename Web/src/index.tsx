import i18next from 'i18next';
import React from 'react';
import ReactDOM from 'react-dom/client';
import { I18nextProvider } from 'react-i18next';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import { API } from './api/API';
import { Caffeine } from './Caffeine';
import { languageConfig } from './languageConfig';

import './styles/index.less';
import { routerConfig } from './utils/routerConfig';

import 'video.js/dist/video-js.css';

i18next.init(languageConfig);
const router = createBrowserRouter(routerConfig(i18next.t), {
    basename: '/kafe',
});

const caffeineObject = new Caffeine(new API());
export const caffeine = React.createContext(caffeineObject);

const root = ReactDOM.createRoot(document.getElementById('root') as HTMLElement);
root.render(
    <React.StrictMode>
        <I18nextProvider i18n={i18next}>
            <caffeine.Provider value={caffeineObject}>
                <RouterProvider router={router} />
            </caffeine.Provider>
        </I18nextProvider>
    </React.StrictMode>,
);
