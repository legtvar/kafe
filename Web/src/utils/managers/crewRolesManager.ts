import CREW_ROLES from './data/crewRoles.json';
import { getStandardOptions, TaggedOption } from "./common";
import { Option } from 'chakra-multiselect';
import { t } from 'i18next';
import { Dispatch, SetStateAction } from 'react';

const CREW_ROLE_TAGS = new Set(<string[]>CREW_ROLES);

const getOptions = (): TaggedOption[] => getStandardOptions('crewRoles', CREW_ROLES);

const getValue = (tags: string[]): TaggedOption[] => tags.map(tag => {
    const name = t(`createProject.fields.crewRoles.${tag}`);
    return { label: name, value: name, tag: tag };
});

const onChange = (value: Option | Option[], setRoles: Dispatch<SetStateAction<string[]>>): void => setRoles(
    (value as TaggedOption[]).map(option => option.tag)
);

const crewRolesManager = {
    getOptions,
    getValue,
    onChange,
    tags: CREW_ROLE_TAGS
};

export default crewRolesManager;
