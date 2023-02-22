import * as React from 'react';
import { Error } from './Error';

interface IErrorBoundaryProps {
    children: React.ReactNode;
}

interface IErrorBoundaryState {
    error: any;
}

export class ErrorBoundary extends React.Component<IErrorBoundaryProps, IErrorBoundaryState> {
    state: IErrorBoundaryState = {
        error: null,
    };

    static getDerivedStateFromError(error: any) {
        return { error };
    }

    render() {
        if (this.state.error) {
            return <Error error={this.state.error} />;
        }
        return this.props.children;
    }
}
