import { Button, FormControl, FormLabel, HStack, Input, Stack, useConst } from '@chakra-ui/react';
import useForceUpdate from 'use-force-update';
import { t } from 'i18next';
import { useNavigate } from 'react-router-dom';
import { API } from '../../../api/API';
import { Organization } from '../../../data/Organization';
import { useAuthLinkFunction } from '../../../hooks/useAuthLink';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { HRIB } from '../../../schemas/generic';
import { LocalizedInput } from '../../utils/LocalizedInput';
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
                        <LocalizedInput
                            as={Input}
                            type="text"
                            borderColor={border}
                            bg={bg}
                            name="name"
                            placeholder={t('createOrganization.fields.name').toString()}
                            value={organization.name}
                            onChange={(value) => forceUpdate(organization.set('name', value))}
                        />
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
