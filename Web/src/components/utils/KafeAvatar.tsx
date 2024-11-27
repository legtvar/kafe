import { Avatar, AvatarProps, useToast } from '@chakra-ui/react';
import { t } from 'i18next';
import { useState } from 'react';
import { forTime } from 'waitasecond';
import { useApi, useAuth } from '../../hooks/Caffeine';
import { avatarUrl } from '../../utils/avatarUrl';
import { Await } from './Await';

export type Person = {
    emailAddress?: string | null;
    id?: string | null;
};

export interface IAvatarProps extends AvatarProps {
    person: Person;
}

export function KafeAvatar({ person, ...props }: IAvatarProps) {
    const [timeoutHandle, setTimeoutHandle] = useState<NodeJS.Timeout | null>(null);
    const [clicks, setClicks] = useState(0);

    const toast = useToast();
    const api = useApi();
    const { user } = useAuth();

    const onTimeout = () => {
        setClicks(0);
    };

    const onClickFullfill = async () => {
        if (!user?.permissions['system']?.includes('write')) {
            return;
        }

        if (!person.id) {
            toast({
                title: t('avatar.clicks.noPerson.title'),
                description: t('avatar.clicks.noPerson.description'),
                status: 'error',
                duration: 3000,
                isClosable: true,
            });
            return;
        }

        setClicks(0);
        toast({
            title: t('avatar.clicks.fullfilled.title'),
            description: t('avatar.clicks.fullfilled.description'),
            status: 'loading',
            duration: 3000,
            isClosable: true,
        });

        await api.accounts.impersonate(person.id!);

        await forTime(3000);

        // Reload the page
        window.location.reload();
    };

    const onClick = () => {
        if (timeoutHandle) {
            clearTimeout(timeoutHandle);
        }
        setTimeoutHandle(setTimeout(onTimeout, 1000));

        const newClicks = clicks + 1;
        setClicks(newClicks);
        if (newClicks >= 10) {
            onClickFullfill();
        }
    };

    return (
        <Await for={avatarUrl(person.emailAddress, person.id)}>
            {(url) => <Avatar src={url} onClick={onClick} {...props} />}
        </Await>
    );
}
