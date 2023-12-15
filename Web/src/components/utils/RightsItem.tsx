import { Avatar, Box, Button, Checkbox, Flex, Input, InputGroup, Text, useColorModeValue } from '@chakra-ui/react';
import { t } from 'i18next';
import { useState } from 'react';
import { IoAdd, IoEarth } from 'react-icons/io5';
import { AbstractType } from '../../data/AbstractType';
import { User } from '../../data/User';
import { useColorScheme } from '../../hooks/useColorScheme';
import { avatarUrl } from '../../utils/avatarUrl';

export interface IRightsItemProps {
    user: User | number | null; // User, 0 for anyone, null for new user
    initialRights: Rights[];
    item: AbstractType | null;
    readonly?: boolean;
}

export enum Rights {
    READ = 'read',
    WRITE = 'write',
    INSPECT = 'inspect',
    APPEND = 'append',
    REVIEW = 'review'
}

export function RightsItem({ user, initialRights, item, readonly }: IRightsItemProps) {
    const borderColor = useColorModeValue('gray.300', 'gray.700');
    const { border, bg } = useColorScheme();
    const [newEmail, setNewEmail] = useState<string>('');
    const [rights, setRights] = useState<Rights[]>(initialRights);

    const rightNames: Record<Rights, string> = {
        read: t('rights.read').toString(),
        write: t('rights.write').toString(),
        inspect: t('rights.inspect').toString(),
        append: t('rights.append').toString(),
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
                    <Avatar size={'sm'} src={avatarUrl(user!.id)} mr={4} />
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
                    {Object.values(Rights).map((right) => (
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
                                isChecked={rights.includes(right)}
                                onChange={(event) => {
                                    if (event.target.checked) {
                                        setRights([...rights, right]);
                                    } else {
                                        setRights(rights.filter((r) => r !== right));
                                    }
                                }}
                            >
                                {rightNames[right]}
                            </Checkbox>
                        </InputGroup>
                    ))}
                </Flex>
            </Box>
            <Button
                colorScheme="blue"
                ml={{
                    base: '0',
                    xl: '4',
                }}
                mt={{
                    base: '2',
                    xl: '0',
                }}
            >
                {t('generic.save').toString()}
            </Button>
        </Flex>
    );
}
