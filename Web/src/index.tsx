import i18next from 'i18next';
import React from 'react';
import ReactDOM from 'react-dom/client';
import { I18nextProvider } from 'react-i18next';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import { API } from './api/API';
import { Caffeine, CaffeineProvider } from './hooks/Caffeine';
import { languageConfig } from './languageConfig';
import { routerConfig } from './routes';

import { ChakraProvider, ColorModeScript } from '@chakra-ui/react';
import moment from 'moment';
import 'moment/locale/cs';
import theme from './theme';
import { storageOrPrompt } from './utils/storageOrPrompt';

(async () => {
    i18next.init(languageConfig);
    i18next.on('languageChanged', function (lng) {
        moment.locale(lng);
    });

    const router = createBrowserRouter(routerConfig(i18next.t), {
        basename: '/',
    });

    const apiCredentials = {
        username: (await storageOrPrompt('dev_username')) as string,
        password: (await storageOrPrompt('dev_password')) as string,
    };

    const caffeine = new Caffeine(new API(apiCredentials));

    const root = ReactDOM.createRoot(document.getElementById('root') as HTMLElement);
    root.render(
        <React.StrictMode>
            <I18nextProvider i18n={i18next}>
                <ChakraProvider theme={theme}>
                    <CaffeineProvider value={caffeine}>
                        <ColorModeScript initialColorMode={theme.config.initialColorMode} />
                        <RouterProvider router={router} />
                    </CaffeineProvider>
                </ChakraProvider>
            </I18nextProvider>
        </React.StrictMode>,
    );
})();
