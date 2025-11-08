import { Avatar, Box, Checkbox, Flex, Input, InputGroup, Text, useColorModeValue } from '@chakra-ui/react';
import { t } from 'i18next';
import { useState } from 'react';
import { IoAdd, IoEarth } from 'react-icons/io5';
import { EntityPermissionsUser } from '../../data/EntityPermissions';
import { useColorScheme } from '../../hooks/useColorScheme';
import { Permission } from '../../schemas/generic';
import { KafeAvatar } from './KafeAvatar';

export interface IPermsEditorItemProps {
    user: EntityPermissionsUser | number | null; // User, 0 for anyone, null for new user
    options: Readonly<Array<Permission>>;
    initialPerms: Array<Permission>;
    onChange: (permissions: Array<Permission>, email?: string) => void;
    readonly?: boolean;
}

export function PermsEditorItem({ user, options, initialPerms, readonly, onChange }: IPermsEditorItemProps) {
    const borderColor = useColorModeValue('gray.300', 'gray.700');
    const { border, bg } = useColorScheme();

    const [newEmail, setNewEmail] = useState<string | undefined>(
        user === 0 ? undefined : user === null ? '' : (user as EntityPermissionsUser).emailAddress,
    );
    const [perms, setPerms] = useState<Permission[]>(initialPerms);

    const onValueChanges = (perms: Permission[], email?: string) => {
        setPerms(perms);
        setNewEmail(email);
        onChange(perms, email);
    };

    const rightNames: Record<Permission, string> = {
        read: t('perms.read').toString(),
        write: t('perms.write').toString(),
        inspect: t('perms.inspect').toString(),
        append: t('perms.append').toString(),
        review: t('perms.review').toString(),
    };

    return (
        <Flex
            direction={{
                base: 'column',
                xl: 'row',
            }}
            mx={-4}
            py={4}
            px={2}
            borderBottomWidth="1px"
            borderBottomColor={borderColor}
            align={{
                base: 'start',
                xl: 'center',
            }}
        >
            {typeof user === 'number' ? (
                {
                    0: (
                        <Flex direction="row" flex="1" align={'center'}>
                            <Avatar
                                backgroundColor={'whiteAlpha.200'}
                                size={'sm'}
                                mr={4}
                                icon={<IoEarth size="1.5em" />}
                            ></Avatar>
                            <Flex direction="column">
                                <Text>{t('perms.special.anyone.title').toString()}</Text>
                                <Text fontSize="smaller" color="red.500">
                                    {t('perms.special.anyone.warning').toString()}
                                </Text>
                            </Flex>
                        </Flex>
                    ),
                }[user]
            ) : user === null ? (
                <Flex direction="row" flex="1" align={'center'}>
                    <Avatar
                        backgroundColor={'whiteAlpha.200'}
                        size={'sm'}
                        mr={4}
                        icon={<IoAdd size="1.5em" />}
                    ></Avatar>
                    <Input
                        w="full"
                        mr={4}
                        placeholder={t('perms.special.new.title').toString()}
                        type="text"
                        value={newEmail}
                        onChange={(event) => onValueChanges(perms, event.target.value)}
                        {...{ border, bg }}
                    />
                </Flex>
            ) : (
                <Flex direction="row" flex="1" align={'center'}>
                    <KafeAvatar size={'sm'} person={user} mr={4} />
                    <Text>{user.emailAddress}</Text>
                </Flex>
            )}
            <Box
                textAlign={{
                    base: 'left',
                    md: 'right',
                }}
                marginTop={{
                    base: '4',
                    xl: '0',
                }}
            >
                <Flex
                    direction={{
                        base: 'column',
                        md: 'row',
                    }}
                >
                    {options.map((right, i) => (
                        <InputGroup key={i}>
                            <Checkbox
                                mr={{
                                    base: '0',
                                    md: '4',
                                }}
                                mb={{
                                    base: '2',
                                    md: '0',
                                }}
                                isDisabled={readonly}
                                isChecked={perms.includes(right)}
                                onChange={(event) => {
                                    if (event.target.checked) {
                                        onValueChanges([...perms, right], newEmail);
                                    } else {
                                        onValueChanges(
                                            perms.filter((r) => r !== right),
                                            newEmail,
                                        );
                                    }
                                }}
                            >
                                {rightNames[right]}
                            </Checkbox>
                        </InputGroup>
                    ))}
                </Flex>
            </Box>
        </Flex>
    );
}
