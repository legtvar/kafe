import { Avatar, AvatarProps, Tooltip } from '@chakra-ui/react';
import { Organization } from '../../../data/Organization';
import { useOrganizations } from '../../../hooks/Caffeine';

export interface IOrganizationAvatarProps extends AvatarProps {
    organization: Organization;
    noHighlight?: boolean;
}

const organizationAvatars: Record<string, string> = {
    lemmafimuni: '/avatars/lemmafimuni.jpg',
    gamesfimuni: '/avatars/gamesfimuni.png',
    'mate-fimuni': '/avatars/mate-fimuni.png',
    'legacy--org': '/avatars/legacy--org.png',
};

export function OrganizationAvatar({ organization, noHighlight, ...props }: IOrganizationAvatarProps) {
    let source = undefined;
    if (organizationAvatars[organization.id]) {
        source = organizationAvatars[organization.id];
    }

    const { currentOrganization } = useOrganizations();

    return (
        <Tooltip hasArrow label={organization.getName()} placement="right">
            <Avatar
                name={organization.getName()}
                src={source}
                shadow={currentOrganization?.id === organization.id && !noHighlight ? 'outline' : undefined}
                {...props}
            />
        </Tooltip>
    );
}
