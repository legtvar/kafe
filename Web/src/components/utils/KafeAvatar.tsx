import { Avatar, AvatarProps } from '@chakra-ui/react';
import { avatarUrl } from '../../utils/avatarUrl';

export type Person = {
    emailAddress?: string | null;
    id?: string | null;
};

export interface IAvatarProps extends AvatarProps {
    person: Person;
}

export function KafeAvatar({ person, ...props }: IAvatarProps) {
    return <Avatar src={avatarUrl(person.emailAddress, person.id)} {...props} />;
}
