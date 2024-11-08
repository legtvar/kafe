import { Avatar, AvatarProps, Tooltip } from '@chakra-ui/react';
import { Organization } from '../../../data/Organization';
import { useOrganizations } from '../../../hooks/Caffeine';
import gamesfimuni from './avatars/gamesfimuni.png';
import legacy from './avatars/legacy--org.png';
import lemmafimuni from './avatars/lemmafimuni.jpg';

export interface IOrganizationAvatarProps extends AvatarProps {
    organization: Organization;
}

const organizationAvatars: Record<string, string> = {
    lemmafimuni,
    gamesfimuni,
    'legacy--org': legacy,
};

export function OrganizationAvatar({ organization, ...props }: IOrganizationAvatarProps) {
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
                shadow={currentOrganization?.id === organization.id ? 'outline' : undefined}
                {...props}
            />
        </Tooltip>
    );
}
