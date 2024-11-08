import { Button, FormControl, FormLabel, HStack, Input, Stack, useConst, useForceUpdate } from '@chakra-ui/react';
import { t } from 'i18next';
import { useNavigate } from 'react-router-dom';
import { API } from '../../../api/API';
import { Organization } from '../../../data/Organization';
import { useAuthLinkFunction } from '../../../hooks/useAuthLink';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { HRIB } from '../../../schemas/generic';
import { getPrefered } from '../../../utils/preferedLanguage';
import { SendAPI } from '../../utils/SendAPI';

interface IOrganizationBasicInfoProps {
    // Cannot be changed after initial draw
    organization?: Organization;
    noSelfSubmit?: boolean;
}

export function OrganizationBasicInfo(props: IOrganizationBasicInfoProps) {
    const { border, bg } = useColorScheme();
    const update = !!props.organization;
    const organization = useConst(props.organization || new Organization({} as any));
    const fu = useForceUpdate();
    const navigate = useNavigate();
    const authLink = useAuthLinkFunction();

    const forceUpdate = (any: any) => {
        fu();
    };

    const sendApiProps = update
        ? {
              onSubmited: (id: HRIB) => {
                  if (update) navigate(0);
                  else navigate(authLink(`/organizations/${id}/edit`));
              },
              value: organization!,
              request: (api: API, value: Organization) => api.organizations.update(value),
          }
        : {
              onSubmited: (id: HRIB) => {
                  navigate(authLink(`/organizations/${id}`));
              },
              value: organization!,
              request: (api: API, value: Organization) => api.organizations.create(value),
          };

    return (
        <SendAPI {...sendApiProps}>
            {(onSubmit, status) => (
                <Stack spacing={8} direction="column" mb={8}>
                    <FormControl>
                        <FormLabel>{t('createOrganization.fields.name').toString()}</FormLabel>
                        <Stack direction={{ base: 'column', md: 'row' }}>
                            <FormControl id="name.cs">
                                <Input
                                    type="text"
                                    borderColor={border}
                                    bg={bg}
                                    placeholder={`${t('createOrganization.fields.name').toString()} ${t(
                                        'createProject.language.cs',
                                    )}`}
                                    defaultValue={getPrefered(organization.name, 'cs')}
                                    onChange={(event) =>
                                        forceUpdate(
                                            organization.set('name', {
                                                ...organization.name,
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
                                    placeholder={`${t('createOrganization.fields.name').toString()} ${t(
                                        'createProject.language.en',
                                    )}`}
                                    defaultValue={getPrefered(organization.name, 'en')}
                                    onChange={(event) =>
                                        forceUpdate(
                                            organization.set('name', {
                                                ...organization.name,
                                                en: event.target.value,
                                            }),
                                        )
                                    }
                                />
                            </FormControl>
                        </Stack>
                    </FormControl>

                    {!props.noSelfSubmit && (
                        <HStack w="100%">
                            <Button
                                colorScheme="blue"
                                ml="auto"
                                isDisabled={status === 'sending' || status === 'ok'}
                                onClick={onSubmit}
                            >
                                {update
                                    ? t('createOrganization.button.update').toString()
                                    : t('createOrganization.button.create').toString()}
                            </Button>
                        </HStack>
                    )}
                </Stack>
            )}
        </SendAPI>
    );
}
