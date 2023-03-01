export function sequence(n: number, from: number = 0) {
    return Array.from(Array(n).keys()).map((i) => i + from);
}
