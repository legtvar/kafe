export function incremental(to: number, from?: number) {
    return [...Array(to - (from || 0)).keys()].map((val) => val + (from || 0));
}
