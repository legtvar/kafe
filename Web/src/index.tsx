import i18next from 'i18next';
import React from 'react';
import ReactDOM from 'react-dom/client';
import { I18nextProvider } from 'react-i18next';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import { API } from './api/API';
import { Caffeine } from './Caffeine';
import { languageConfig } from './languageConfig';
import { routerConfig } from './utils/routerConfig';

import { ChakraProvider, ColorModeScript } from '@chakra-ui/react';
import moment from 'moment';
import 'moment/locale/cs';
import 'video.js/dist/video-js.css';
import theme from './theme';

i18next.init(languageConfig);
i18next.on('languageChanged', function (lng) {
    moment.locale(lng);
});

const router = createBrowserRouter(routerConfig(i18next.t), {
    basename: '/kafe',
});

const caffeineObject = new Caffeine(new API());
export const caffeine = React.createContext(caffeineObject);

const root = ReactDOM.createRoot(document.getElementById('root') as HTMLElement);
root.render(
    <React.StrictMode>
        <I18nextProvider i18n={i18next}>
            <ChakraProvider theme={theme}>
                <caffeine.Provider value={caffeineObject}>
                    <ColorModeScript initialColorMode={theme.config.initialColorMode} />
                    <RouterProvider router={router} />
                </caffeine.Provider>
            </ChakraProvider>
        </I18nextProvider>
    </React.StrictMode>,
);
