import React from 'react';

export function useTitle(title?: string | null) {
    if (typeof document === 'undefined') {
        return;
    }

    const originalTitle = React.useRef(document.title);

    React.useEffect(() => {
        document.title = title ? `${title} | KAFE` : 'KAFE';
        return () => {
            document.title = originalTitle.current;
        };
    }, []);
}
