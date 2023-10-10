import { Button, FormControl, FormLabel, HStack, Input, Stack, useConst, useForceUpdate } from '@chakra-ui/react';
import { t } from 'i18next';
import { useNavigate } from 'react-router-dom';
import { API } from '../../../api/API';
import { Group } from '../../../data/Group';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { HRIB } from '../../../schemas/generic';
import { getPrefered } from '../../../utils/preferedLanguage';
import { SendAPI } from '../../utils/SendAPI';
import { TextareaMarkdown } from '../../utils/TextareaMarkdown';

interface IGroupBasicInfoProps {
    // Cannot be changed after initial draw
    group?: Group;
}

export function GroupBasicInfo(props: IGroupBasicInfoProps) {
    const { border, bg } = useColorScheme();
    const update = !!props.group;
    const group = useConst(props.group || new Group({} as any));
    const fu = useForceUpdate();
    const navigate = useNavigate();

    const forceUpdate = (any: any) => {
        fu();
    };

    const sendApiProps = update
        ? {
              onSubmited: (id: HRIB) => {
                  if (update) navigate(0);
                  else navigate(`/auth/groups/${id}/edit`);
              },
              value: group!,
              request: (api: API, value: Group) => api.groups.update(value),
          }
        : {
              onSubmited: (id: HRIB) => {
                  navigate(`/auth/groups/${id}`);
              },
              value: group!,
              request: (api: API, value: Group) => api.groups.create(value),
          };

    return (
        <SendAPI {...sendApiProps}>
            {(onSubmit, status) => (
                <Stack spacing={8} direction="column" mb={8}>
                    <FormControl>
                        <FormLabel>{t('createGroup.fields.name').toString()}</FormLabel>
                        <Stack direction={{ base: 'column', md: 'row' }}>
                            <FormControl id="name.cs">
                                <Input
                                    type="text"
                                    borderColor={border}
                                    bg={bg}
                                    placeholder={`${t('createGroup.fields.name').toString()} ${t(
                                        'createProject.language.cs',
                                    )}`}
                                    defaultValue={getPrefered(group.name, 'cs')}
                                    onChange={(event) =>
                                        forceUpdate(
                                            group.set('name', {
                                                ...group.name,
                                                cs: event.target.value,
                                            }),
                                        )
                                    }
                                />
                            </FormControl>

                            <FormControl id="name.en">
                                <Input
                                    type="text"
                                    borderColor={border}
                                    bg={bg}
                                    placeholder={`${t('createGroup.fields.name').toString()} ${t(
                                        'createProject.language.en',
                                    )}`}
                                    defaultValue={getPrefered(group.name, 'en')}
                                    onChange={(event) =>
                                        forceUpdate(
                                            group.set('name', {
                                                ...group.name,
                                                en: event.target.value,
                                            }),
                                        )
                                    }
                                />
                            </FormControl>
                        </Stack>
                    </FormControl>

                    <FormControl>
                        <FormLabel>{t('createGroup.fields.description').toString()}</FormLabel>
                        <Stack direction={{ base: 'column', md: 'row' }}>
                            <FormControl id="description.cs">
                                <TextareaMarkdown
                                    placeholder={`${t('createGroup.fields.description').toString()} ${t(
                                        'createProject.language.cs',
                                    )}`}
                                    borderColor={border}
                                    bg={bg}
                                    defaultValue={getPrefered(group.description, 'cs')}
                                    onChange={(event) =>
                                        forceUpdate(
                                            group.set('description', {
                                                ...group.description,
                                                cs: event.target.value,
                                            }),
                                        )
                                    }
                                />
                            </FormControl>

                            <FormControl id="description.en">
                                <TextareaMarkdown
                                    placeholder={`${t('createGroup.fields.description').toString()} ${t(
                                        'createProject.language.en',
                                    )}`}
                                    borderColor={border}
                                    bg={bg}
                                    defaultValue={getPrefered(group.description, 'en')}
                                    onChange={(event) =>
                                        forceUpdate(
                                            group.set('description', {
                                                ...group.description,
                                                en: event.target.value,
                                            }),
                                        )
                                    }
                                />
                            </FormControl>
                        </Stack>
                    </FormControl>

                    <FormControl>
                        <FormLabel>{t('createGroup.fields.propositions').toString()}</FormLabel>
                        <Stack direction={{ base: 'column', md: 'row' }}>
                            <FormControl id="propositions.cs">
                                <TextareaMarkdown
                                    placeholder={`${t('createGroup.fields.propositions').toString()} ${t(
                                        'createProject.language.cs',
                                    )}`}
                                    borderColor={border}
                                    bg={bg}
                                    defaultValue={getPrefered(group.description, 'cs')}
                                    onChange={(event) =>
                                        forceUpdate(
                                            group.set('customFields', {
                                                ...group.customFields,
                                                propositions: {
                                                    ...group.customFields.propositions,
                                                    cs: event.target.value,
                                                },
                                            }),
                                        )
                                    }
                                />
                            </FormControl>

                            <FormControl id="propositions.en">
                                <TextareaMarkdown
                                    placeholder={`${t('createGroup.fields.propositions').toString()} ${t(
                                        'createProject.language.en',
                                    )}`}
                                    borderColor={border}
                                    bg={bg}
                                    defaultValue={getPrefered(group.description, 'en')}
                                    onChange={(event) =>
                                        forceUpdate(
                                            group.set('customFields', {
                                                ...group.customFields,
                                                propositions: {
                                                    ...group.customFields.propositions,
                                                    en: event.target.value,
                                                },
                                            }),
                                        )
                                    }
                                />
                            </FormControl>
                        </Stack>
                    </FormControl>

                    <HStack w="100%">
                        <Button
                            colorScheme="blue"
                            ml="auto"
                            isDisabled={status === 'sending' || status === 'ok'}
                            onClick={onSubmit}
                        >
                            {update
                                ? t('createGroup.button.update').toString()
                                : t('createGroup.button.create').toString()}
                        </Button>
                    </HStack>
                </Stack>
            )}
        </SendAPI>
    );
}
