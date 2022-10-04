export function classNames(...classes: Array<string | undefined | false | null>) {
    return classes.filter((className) => className).join(' ');
}
