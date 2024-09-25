import i18next from 'i18next';
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
import { CookiesProvider } from 'react-cookie';
import { overrideConsole } from './consoleOverride';
import theme from './theme';

overrideConsole();

(async () => {
    i18next.init(languageConfig);
    i18next.on('languageChanged', function (lng) {
        moment.locale(lng);
    });

    const router = createBrowserRouter(routerConfig(i18next.t), {
        basename: '/',
    });

    const caffeine = new Caffeine(new API());

    const root = ReactDOM.createRoot(document.getElementById('root') as HTMLElement);
    root.render(
        <I18nextProvider i18n={i18next}>
            <CookiesProvider>
                <ChakraProvider theme={theme}>
                    <CaffeineProvider value={caffeine}>
                        <ColorModeScript initialColorMode={theme.config.initialColorMode} />
                        <RouterProvider router={router} />
                    </CaffeineProvider>
                </ChakraProvider>
            </CookiesProvider>
        </I18nextProvider>,
    );
})();
