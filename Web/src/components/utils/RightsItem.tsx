import { Avatar, Box, Button, Checkbox, Flex, Input, InputGroup, Text, useColorModeValue } from '@chakra-ui/react';
import { t } from 'i18next';
import { useEffect, useState } from 'react';
import { IoAdd, IoEarth } from 'react-icons/io5';
import { AbstractType } from '../../data/AbstractType';
import { User } from '../../data/User';
import { useColorScheme } from '../../hooks/useColorScheme';
import { avatarUrl } from '../../utils/avatarUrl';
import { HRIB, Permission } from '../../schemas/generic';

export interface IUser {
    id?: HRIB;
    emailAddress?: string;
    name?: string;
    permissions: Array<Permission>;
}

export interface IRightsItemProps {
    user: IUser | number | null; // User, 0 for anyone, null for new user
    options: Readonly<Array<Permission>>;
    initialPerms: Array<Permission>;
    onChange: (user: IUser) => void;
    readonly?: boolean;
}

export function RightsItem({ user, options, initialPerms, readonly, onChange }: IRightsItemProps) {
    const borderColor = useColorModeValue('gray.300', 'gray.700');
    const { border, bg } = useColorScheme();
    const [newEmail, setNewEmail] = useState<string>('');
    const [perms, setPerms] = useState<Permission[]>(initialPerms);

    useEffect(() => {
        let newUser: IUser = typeof user === 'number' ? { permissions: [] } : { ...user, permissions: [] };
        newUser.emailAddress = newEmail;
        newUser.permissions = perms;
        onChange(newUser);
    }, [perms, newEmail]);

    const rightNames: Record<Permission, string> = {
        read: t('rights.read').toString(),
        write: t('rights.write').toString(),
        inspect: t('rights.inspect').toString(),
        append: t('rights.append').toString(),
        review: t('rights.review').toString(),
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
                                <Text>{t('rights.special.anyone.title').toString()}</Text>
                                <Text fontSize="smaller" color="red.500">
                                {t('rights.special.anyone.warning').toString()}
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
                        placeholder={t('rights.special.new.title').toString()}
                        type="text"
                        value={newEmail}
                        onChange={(event) => setNewEmail(event.target.value)}
                        {...{ border, bg }}
                    />
                </Flex>
            ) : (
                <Flex direction="row" flex="1" align={'center'}>
                    <Avatar size={'sm'} src={avatarUrl(user!.id ?? null)} mr={4} />
                    <Text>{user.name || user.emailAddress}</Text>
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
                    {options.map((right) => (
                        <InputGroup>
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
                                        setPerms([...perms, right]);
                                    } else {
                                        setPerms(perms.filter((r) => r !== right));
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
