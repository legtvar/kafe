export function corsProxy(url: string) {
    return `https://tools.xrosecky.cz/proxy/?csurl=${encodeURIComponent(url)}`;
}
