import { SHA256 } from 'crypto-js';

export function avatarUrl(email?: string | null, id?: string | null) {
    if (email) {
        // if (email.endsWith('@mail.muni.cz')) {
        //     const isImage = await axios.get(`https://is.muni.cz/lide/foto?uco=${email.split('@')[0]}`, {
        //         responseType: 'stream'
        //     });
        //     const invalidIsImage = await axios.get(`https://is.muni.cz/lide/foto?uco=1`, {
        //         responseType: 'stream'
        //     });

        //     // Compare SHA256(email) with SHA256('1') to check if the image is invalid
        //     const sha256Email = SHA256(email).toString();
        //     const sha256Invalid = SHA256('1').toString();

        //     if (isImage.status === 200 && sha256Email !== sha256Invalid) {
        //         return `https://is.muni.cz/lide/foto?uco=${email.split('@')[0]}`;
        //     }
        // }

        const hash = SHA256(email).toString();
        const fallback = `https://api.dicebear.com/5.x/shapes/png/seed=${hash}`;
        const url = `https://www.gravatar.com/avatar/${hash}?d=${encodeURIComponent(fallback)}`;

        return url;
    }

    if (id) {
        return `https://api.dicebear.com/5.x/shapes/png/seed=${id}`;
    }

    return `https://api.dicebear.com/5.x/shapes/png`;
}
