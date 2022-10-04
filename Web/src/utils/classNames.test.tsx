import { classNames } from './classNames';

test('ClassNames', () => {
    expect(classNames('a', 'b', 'c')).toBe('a b c');
    expect(classNames()).toBe('');
    expect(classNames('a', false, 'c')).toBe('a c');
    expect(classNames(false)).toBe('');
});
