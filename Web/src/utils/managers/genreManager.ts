import genres from './data/genres.json';
import { getStandardOptions, TaggedOption, translateSingle, USER_DEFINED_ITEM_TAG } from "./common";
import { Option } from 'chakra-multiselect';
import { Project } from '../../data/Project';
import { t } from 'i18next';
import { localizedString } from '@/schemas/generic';

const MAX_GENRE_COUNT = 4;
const MAX_GENRE_LENGTH = 32;

const getOptions = (): TaggedOption[] => getStandardOptions('genres', genres)

const getValue = (project: Project): TaggedOption[] => {

    const localizedString = project.genre;

    if (!localizedString) {
        return [];
    }

    const ivNames = localizedString.iv.split(',');

    return localizedString.tags.split(',').map((tag, i) => {

        const name = tag === USER_DEFINED_ITEM_TAG
            ? ivNames[i] : t(`createProject.fields.genres.${tag}`).toString();

        return { label: name, value: name, tag: tag };
    });
}

const onChange = (value: Option | Option[], project: Project, fu: (x: any) => void): void => {

    const values = (value as TaggedOption[])

    // Can not have no genres!
    if (values.length === 0) {
        fu(project.set('genre', undefined));
        return;
    }

    // Can not have more than MAX_GENRE_COUNT genres!
    if (values.length > MAX_GENRE_COUNT) {
        return;
    }

    // Can not have genre with label length greater than MAX_GENRE_LENGTH!
    if (values.filter(value => value.label.length > MAX_GENRE_LENGTH).length != 0) {
        return;
    }

    // Can not have genre with label containing comma (',')!
    if (values.filter(value => value.label.includes(',')).length != 0) {
        return;
    }

    // Can not have genre with empty label!
    if (values.filter(value => value.label.trim().length == 0).length != 0) {
        return;
    }

    const aux: { [key: string]: string[] } = {
        tags: [], iv: [], en: [], cs: [], sk: []
    };

    values.map(value => Object.keys(aux).map(key => aux[key].push(
        key === 'tags'
            ? (!value.tag || value.tag === USER_DEFINED_ITEM_TAG
                ? USER_DEFINED_ITEM_TAG : value.tag
            )
            : (!value.tag || value.tag === USER_DEFINED_ITEM_TAG
                ? value.label.trim() : translateSingle(
                    (key === 'iv' ? 'en' : key), 'genres', value.tag
                )
            )
    )
    )
    );

    const result: localizedString = {};

    Object.keys(aux).map(key =>
        result[key] = aux[key].join(key === 'tags' ? ',' : ', '));

    fu(project.set('genre', result));
}

const genreManager = {
    getOptions,
    getValue,
    onChange
};

export default genreManager;
