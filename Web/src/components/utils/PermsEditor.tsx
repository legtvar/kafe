import { Box, List, Spacer, Text } from '@chakra-ui/react';
import { t } from 'i18next';
import { useState } from 'react';
import { EntityPermissions, EntityPermissionsUser } from '../../data/EntityPermissions';
import { useColorScheme } from '../../hooks/useColorScheme';
import { Permission } from '../../schemas/generic';
import { PermsEditorItem } from './PermsEditorItem';

export interface IPermsEditor {
    perms: EntityPermissions;
    readonly?: boolean;
    options: Readonly<Array<Permission>>;
    explanation: Partial<Record<Permission, string>>;
    onChange?: (perms: EntityPermissions) => void;
}

export function PermsEditor({ perms, readonly, explanation, options, onChange }: IPermsEditor) {
    const { border } = useColorScheme();

    const [newItems, setNewItems] = useState<Array<EntityPermissionsUser>>([
        {
            emailAddress: '',
            permissions: [],
        },
    ]);

    return (
        <>
            <Box mb={4} mx={-4} fontSize="smaller" color="gray.500">
                <List>
                    {options.map((o, i) => (
                        <li key={i}>
                            <Text as="span" fontWeight="bold">
                                {t(`perms.${o}`).toString()}
                            </Text>
                            <Text as="span"> - {explanation[o]}</Text>
                        </li>
                    ))}
                </List>
            </Box>
            <Box>
                {perms.globalPermissions !== null && (
                    <PermsEditorItem
                        user={0}
                        initialPerms={perms.globalPermissions}
                        {...{ readonly, options }}
                        onChange={(permissions) => {
                            perms.set('globalPermissions', permissions);

                            onChange?.(perms);
                        }}
                    />
                )}

                <Spacer borderBottomColor={border} borderBottomWidth={1} mx={-4} mt={1} />

                {perms.accountPermissions
                    .filter((a) => a.id)
                    .map((a, i) => (
                        <PermsEditorItem
                            key={i}
                            user={a}
                            initialPerms={a.permissions}
                            {...{ readonly, options }}
                            onChange={(permissions) => {
                                perms.accountPermissions
                                    .filter((b) => b.id === a.id)
                                    .forEach((b) => (b.permissions = permissions));
                                perms.set('accountPermissions', perms.accountPermissions);

                                onChange?.(perms);
                            }}
                        />
                    ))}

                <Spacer borderBottomColor={border} borderBottomWidth={1} mx={-4} mt={1} />

                {newItems.map((item, i) => (
                    <PermsEditorItem
                        key={i}
                        user={null}
                        initialPerms={[]}
                        {...{ readonly, options }}
                        onChange={(permissions, email) => {
                            newItems[i] = {
                                emailAddress: email || '',
                                permissions,
                            };
                            if (newItems.length === 0 || newItems.at(-1)!.emailAddress.length > 0) {
                                newItems.push({
                                    emailAddress: '',
                                    permissions: [],
                                });
                            }
                            setNewItems([...newItems]);

                            perms.accountPermissions = [
                                ...perms.accountPermissions.filter((a) => a.id),
                                ...(newItems.filter((a) => a.emailAddress) as EntityPermissionsUser[]),
                            ];
                            perms.set('accountPermissions', perms.accountPermissions);

                            onChange?.(perms);
                        }}
                    />
                ))}
            </Box>
        </>
    );
}
