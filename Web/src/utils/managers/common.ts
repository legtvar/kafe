import { Option } from 'chakra-multiselect';
import { resources } from '../../translations/resources';
import { t } from 'i18next';

export const USER_DEFINED_ITEM_TAG = '0';

export type TaggedOption = Option & {
    tag: string
}

export const getStandardOptions = (from: string, tags: string[]): TaggedOption[] => tags.map(tag => {
    const name = t(`createProject.fields.${from}.${tag}`).toString();
    return { label: name, value: name, tag: tag };
}).sort((a, b) => a.label.localeCompare(b.label))

export const translateSingle = (lng: string, what: string, tag: string): string =>
    (resources as any)[lng].common.createProject.fields[what][tag];
