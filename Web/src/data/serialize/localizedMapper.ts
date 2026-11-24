import { localizedString } from '../../schemas/generic';

export function localizedMapper(localized: localizedString): Record<string, string> | undefined {
    if (!localized) {
        return undefined;
    }

    return Object.fromEntries(Object.entries(localized).filter(([key, text]) => text && text.length > 0));
};
