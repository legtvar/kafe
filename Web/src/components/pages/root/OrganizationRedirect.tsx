import { Navigate } from 'react-router-dom';
import { useOrganizations } from '../../../hooks/Caffeine';
import { OutletOrChildren } from '../../utils/OutletOrChildren';

export const LS_LATEST_ORG_KEY = 'kafe_latest_org';

export function OrganizationRedirect() {
    let org = localStorage.getItem(LS_LATEST_ORG_KEY);
    const { organizations } = useOrganizations();

    if (!org) {
        org = organizations[0].id;
        localStorage.setItem(LS_LATEST_ORG_KEY, org);
    }

    return (
        <OutletOrChildren>
            <Navigate to={org} />
        </OutletOrChildren>
    );
}
