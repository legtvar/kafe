import { components } from '../../schemas/api';

export const rolesMapper = (people: components['schemas']['ProjectAuthorDto'][]) =>
    (people || []).map((person) => ({
        id: person.id,
        roles: person.roles,
    }));
