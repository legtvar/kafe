import localforage from 'localforage';

export async function storageOrPrompt(key: string) {
    try {
        const result = await localforage.getItem(key);
        if (!result) throw Error();
        return result;
    } catch {
        let prompt;
        do {
            prompt = window.prompt(`Please enter value for "${key}"`, '');
        } while (prompt === null || prompt === '');
        localforage.setItem(key, prompt);
        return prompt;
    }
}
