import GENRES from './data/genres.json';
import { getStandardOptions, TaggedOption, translateSingle, USER_DEFINED_ITEM_TAG } from "./common";
import { Option } from 'chakra-multiselect';
import { Project } from '../../data/Project';
import { t } from 'i18next';
import { localizedString } from '@/schemas/generic';

const GENRE_TAGS = new Set(<string[]>GENRES);

const MAX_GENRE_COUNT = 4;
const MAX_GENRE_LENGTH = 32;

const getOptions = (): TaggedOption[] => getStandardOptions('genres', GENRES)

const getValue = (genres: localizedString | undefined): TaggedOption[] | undefined => {

    if (!genres) {
        return [];
    }
    
    if (!genres.iv) {
        return undefined;
    }

    return genres['iv'].split(',').map((tag, i) => {

        tag = tag.trim();
        let name = tag;
        if (GENRE_TAGS.has(tag))
        {
            name = t(`createProject.fields.genres.${tag}`).toString();
        }
        else
        {
            name = tag;
            tag = USER_DEFINED_ITEM_TAG;
        }

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
        iv: [], en: [], cs: [], sk: []
    };

    values.map(value => Object.keys(aux).map(key => aux[key].push(
        key === 'iv'
            ? (!value.tag || value.tag === USER_DEFINED_ITEM_TAG
                ? value.label.trim() : value.tag
            )
            : (!value.tag || value.tag === USER_DEFINED_ITEM_TAG
                ? value.label.trim() : translateSingle(
                    key, 'genres', value.tag
                )
            )
    )));

    const result: localizedString = {};

    Object.keys(aux).map(key =>
        result[key] = aux[key].join(', '));

    fu(project.set('genreTags', values));
    fu(project.set('genre', result));
}

const genreManager = {
    getOptions,
    getValue,
    onChange
};

export default genreManager;
