import common_cs from './cs/common.json';
import common_en from './en/common.json';
import common_sk from './sk/common.json';

export const resources = {
    en: {
        common: {
            ...common_en,
        },
    },
    cs: {
        common: {
            ...common_cs,
        },
    },
    sk: {
        common: {
            ...common_sk,
        },
    },
};
