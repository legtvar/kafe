export function deepCopy(data: any) {
    return JSON.parse(JSON.stringify(data));
}
