import MD5 from 'crypto-js/md5';
import { User } from '../data/User';

export function avatarUrl(user: User) {
    const hash = MD5(user.email).toString();

    const fallback = `https://api.dicebear.com/5.x/shapes/svg?seed=${hash}`;

    return `https://www.gravatar.com/avatar/${hash}?d=${encodeURIComponent(fallback)}`;
}
