export const currentOrganizationIdMapper = () => {
    const location = window.location;
    const orgId = location.pathname.split('/')[2];

    return orgId;
};
