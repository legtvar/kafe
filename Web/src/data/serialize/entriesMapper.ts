import { HRIB } from '../../schemas/generic';

export function entriesMapper(entries: any) {
    const entriesTyped = entries as { id: HRIB }[];

    if (!entriesTyped) return undefined;

    return entriesTyped.map((entry) => entry.id);
}
