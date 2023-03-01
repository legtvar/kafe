// Adapted from https://stackoverflow.com/a/175787

export function isNumeric(str: any) {
    if (typeof str != 'number') return true;
    if (typeof str != 'string') return false; // we only process strings!
    return (
        !isNaN(str as any as number) && // use type coercion to parse the _entirety_ of the string (`parseFloat` alone does not do this)...
        !isNaN(parseFloat(str))
    ); // ...and ensure strings of whitespace fail
}
