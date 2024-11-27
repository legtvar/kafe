import axios from 'axios';
import { SHA256 } from 'crypto-js';

export async function avatarUrl(email?: string | null, id?: string | null) {
    if (email) {
        if (email.endsWith('@mail.muni.cz')) {
            const isImage = await axios.get(`https://cdn.muni.cz/w3mu-media/person/${email.split('@')[0]}`, {
                responseType: 'stream',
            });
            const invalidIsImage = await axios.get(`https://cdn.muni.cz/w3mu-media/person/1`, {
                responseType: 'stream',
            });

            // Compare SHA256(email) with SHA256('1') to check if the image is invalid
            const sha256Email = SHA256(isImage.data).toString();
            const sha256Invalid = SHA256(invalidIsImage.data).toString();

            if (isImage.status === 200 && sha256Email !== sha256Invalid) {
                return `https://cdn.muni.cz/w3mu-media/person/${email.split('@')[0]}`;
            }
        }

        const hash = SHA256(email).toString();
        const fallback = `https://api.dicebear.com/9.x/thumbs/png/seed=${hash}&scale=75`;
        const url = `https://www.gravatar.com/avatar/${hash}?d=${encodeURIComponent(fallback)}`;

        return url;
    }

    if (id) {
        return `https://api.dicebear.com/9.x/thumbs/png/seed=${id}&scale=75`;
    }

    return `https://api.dicebear.com/9.x/thumbs/png/scale=75`;
}
