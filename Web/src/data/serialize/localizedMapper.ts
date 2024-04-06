import { localizedString } from '../../schemas/generic';

export const localizedMapper = (localized: localizedString) => {
    if (!localized) return undefined;

    const cs = localized['cs'] && localized['cs'].length > 0 ? localized['cs'] : undefined;
    const en = localized['en'] && localized['en'].length > 0 ? localized['en'] : undefined;

    // Force set invariant to english for now...
    // const iv = localized['iv'] && localized['iv'].length > 0 ? localized['iv'] : cs || en || undefined;
    const iv = localized['en'] || localized['cs'] || '';

    if (!iv) return undefined;

    const res: localizedString = { iv };

    if (cs) res['cs'] = cs;
    if (en) res['en'] = en;

    return res;
};
