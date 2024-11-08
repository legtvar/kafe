import { useLocation } from 'react-router-dom';

export function useAuthLink(to?: string, organization?: string) {
    // Get curretn path from router
    const location = useLocation();

    const split = location.pathname.split('/');

    // Take first three parts of the path
    let base = split.slice(0, 3).join('/');
    if (organization) {
        base = split.slice(0, 2).join('/') + '/' + organization;
    }

    const page = '/' + split.slice(3).join('/');

    if (to) {
        return base + to;
    }

    return base + page;
}
