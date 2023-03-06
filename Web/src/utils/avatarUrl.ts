import MD5 from 'crypto-js/md5';

export function avatarUrl(email: string | null, id?: string) {
    if (email) {
        const hash = MD5(email).toString();
        const fallback = `https://api.dicebear.com/5.x/shapes/png/seed=${hash}`;
        const url = `https://www.gravatar.com/avatar/${hash}?d=${encodeURIComponent(fallback)}`;
        // const url = `https://www.gravatar.com/avatar/${hash}?d=404`;

        // console.log(url);
        return url;
    }

    if (id) {
        return `https://api.dicebear.com/5.x/shapes/png/seed=${id}`;
    }

    return `https://api.dicebear.com/5.x/shapes/png`;
}
