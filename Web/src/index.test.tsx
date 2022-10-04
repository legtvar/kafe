import { render } from '@testing-library/react';
import { Root } from './components/layout/Root';

test('Fake test', () => {
    render(<Root />);

    expect(true).toBeTruthy();
});
