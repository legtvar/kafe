import { extendTheme, StyleProps } from '@chakra-ui/react';
import { mode } from '@chakra-ui/theme-tools';
import { MultiSelectTheme } from 'chakra-multiselect';

// see https://chakra-ui.com/docs/styled-system/theme
const theme = extendTheme({
    components: {
        MultiSelect: {
            ...MultiSelectTheme,
            baseStyle: (props: Record<string, any>) => {
                const bs = MultiSelectTheme.baseStyle(props);
                return {
                    ...bs,
                    control: {
                        ...bs.control, bgColor: mode('white', 'gray.900')(props)
                    }
                }
            }
        }
    },
    config: {
        initialColorMode: 'light',
        useSystemColorMode: false,
        cssVarPrefix: 'kafe',
    },
    colors: {
        brand: {
            50: '#ebf8ff',
            100: '#bee3f8',
            200: '#90cdf4',
            300: '#63b3ed',
            400: '#4299e1',
            500: '#3182ce',
            600: '#2b6cb0',
            700: '#2c5282',
            800: '#2a4365',
            900: '#1A365D',
        },
        grayAlpha: {
            50: 'rgba(23,25,35,0.04)',
            100: 'rgba(23,25,35,0.06)',
            200: 'rgba(23,25,35,0.08)',
            300: 'rgba(23,25,35,0.16)',
            400: 'rgba(23,25,35,0.24)',
            500: 'rgba(23,25,35,0.36)',
            600: 'rgba(23,25,35,0.48)',
            700: 'rgba(23,25,35,0.64)',
            800: 'rgba(23,25,35,0.80)',
            900: 'rgba(23,25,35,0.92)',
        },
        gray: {
            650: '#374358',
        },
    },
    fonts: {
        body: "'Open Sans', sans-serif",
        heading: "'Open Sans', sans-serif",
        mono: "Consolas, 'Courier New', sans-serif",
    },
    styles: {
        global: (props: StyleProps) => ({
            body: {
                color: mode('gray.800', 'whiteAlpha.900')(props),
                bg: mode('gray.100', 'gray.800')(props),
                overflow: 'hidden',
            },
            html: {
                overflow: 'hidden',
            },
        }),
    },
});

export default theme;
