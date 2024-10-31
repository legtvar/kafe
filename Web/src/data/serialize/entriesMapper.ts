import { HRIB } from '../../schemas/generic';

export const entriesMapper = (entries: { id: HRIB }[]) => {
    if (!entries) return undefined;

    return entries.map((entry) => entry.id);
};
