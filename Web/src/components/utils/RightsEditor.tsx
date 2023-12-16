import { Box, List, Spacer, Text } from '@chakra-ui/react';
import { t } from 'i18next';
import { AbstractType } from '../../data/AbstractType';
import { useAuth } from '../../hooks/Caffeine';
import { useColorScheme } from '../../hooks/useColorScheme';
import { Rights, RightsItem } from './RightsItem';
import { EntityPermissions, EntityPermissionsUser } from '../../data/EntityPermissions';
import { useState } from 'react';

export interface IRightsEditorProps {
    perms: EntityPermissions;
    readonly?: boolean;
    options: Array<Rights>;
    explanation: Record<Rights, string>;
}

export function RightsEditor({ perms, readonly, explanation, options }: IRightsEditorProps) {
    const { border } = useColorScheme();

    const [newItems, setNewItems] = useState<Array<Partial<EntityPermissionsUser>>>([{}]);

    return (
        <>
            <Box mb={4} mx={-4} fontSize="smaller" color="gray.500">
                <List>
                    {options.map((o) => (
                        <li>
                            <Text as="span" fontWeight="bold">
                                {t(`rights.${o}`).toString()}
                            </Text>
                            <Text as="span"> - {explanation[o]}</Text>
                        </li>
                    ))}
                </List>
            </Box>
            <Box>
                {perms.globalPermissions && (
                    <RightsItem
                        user={0}
                        initialPerms={perms.globalPermissions as Array<Rights>}
                        {...{ readonly, options }}
                        onChange={(user) => perms.set('globalPermissions', user.permissions)}
                    />
                )}

                <Spacer borderBottomColor={border} borderBottomWidth={1} mx={-4} mt={1} />

                {perms.accountPermissions
                    .filter((a) => a.id)
                    .map((a) => (
                        <RightsItem
                            user={a}
                            initialPerms={a.permissions as Array<Rights>}
                            {...{ readonly, options }}
                            onChange={(user) => {
                                perms.accountPermissions
                                    .filter((b) => b.id === a.id)
                                    .forEach((b) => (b.permissions = user.permissions));
                                perms.changed.add('accountPermissions');
                            }}
                        />
                    ))}

                <Spacer borderBottomColor={border} borderBottomWidth={1} mx={-4} mt={1} />

                {newItems.map((a, i) => (
                    <RightsItem
                        key={i}
                        user={null}
                        initialPerms={[]}
                        {...{ readonly, options }}
                        onChange={(user) => {
                            let newNewItems = newItems.map((n, j) => (i == j ? user : n));

                            if (newNewItems.filter((e) => !e.emailAddress).length == 0) {
                                newNewItems.push({});
                            }
                            setNewItems(newNewItems);

                            perms.accountPermissions = [
                                ...perms.accountPermissions.filter((a) => a.id),
                                ...(newNewItems.filter(a => a.emailAddress) as EntityPermissionsUser[]),
                            ];
                            console.log(perms);
                        }}
                    />
                ))}
            </Box>
        </>
    );
}
