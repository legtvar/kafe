import { Navigate, NavigateProps, useLocation } from 'react-router-dom';

const LS_KEY = 'kafe_backlink';

export function NavigateStoreBacklink(props: NavigateProps) {
    // Get current location from React Router
    const location = useLocation().pathname;

    // Push current location to local storage
    const stack = JSON.parse(localStorage.getItem(LS_KEY) || '[]');
    stack.push(location);
    localStorage.setItem(LS_KEY, JSON.stringify(stack));

    return <Navigate {...props} />;
}

export function NavigateRestoreBacklink({ to, ...props }: NavigateProps) {
    // Get last location from local storage
    const stack = JSON.parse(localStorage.getItem(LS_KEY) || '[]') as string[];
    const location = stack.pop();
    localStorage.setItem(LS_KEY, JSON.stringify(stack));

    return <Navigate to={location || to} {...props} />;
}
