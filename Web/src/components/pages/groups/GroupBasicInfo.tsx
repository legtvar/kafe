import {
    Button,
    Checkbox,
    FormControl,
    FormLabel,
    Heading,
    HStack,
    Input,
    Stack,
    useConst,
    useForceUpdate,
} from '@chakra-ui/react';
import { t } from 'i18next';
import moment from 'moment';
import { useNavigate } from 'react-router-dom';
import { API } from '../../../api/API';
import { Group } from '../../../data/Group';
import { useAuthLinkFunction } from '../../../hooks/useAuthLink';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { HRIB } from '../../../schemas/generic';
import { LocalizedInput } from '../../utils/LocalizedInput';
import { SendAPI } from '../../utils/SendAPI';
import { TextareaMarkdown } from '../../utils/TextareaMarkdown';

interface IGroupBasicInfoProps {
    // Cannot be changed after initial draw
    group?: Group;
    noSelfSubmit?: boolean;
}

export function GroupBasicInfo(props: IGroupBasicInfoProps) {
    const { border, bg } = useColorScheme();
    const update = !!props.group;
    const group = useConst(props.group || new Group({} as any));
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
                  else navigate(authLink(`/groups/${id}/edit`));
              },
              value: group!,
              request: (api: API, value: Group) => api.groups.update(value),
          }
        : {
              onSubmited: (id: HRIB) => {
                  navigate(authLink(`/groups/${id}`));
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
                        <LocalizedInput
                            as={Input}
                            type="text"
                            borderColor={border}
                            bg={bg}
                            name="name"
                            placeholder={t('createGroup.fields.name').toString()}
                            value={group.name}
                            onChange={(value) => forceUpdate(group.set('name', value))}
                        />
                    </FormControl>

                    <FormControl>
                        <FormLabel>{t('createGroup.fields.description').toString()}</FormLabel>
                        <LocalizedInput
                            as={TextareaMarkdown}
                            borderColor={border}
                            bg={bg}
                            name="description"
                            placeholder={t('createGroup.fields.description').toString()}
                            value={group.description}
                            onChange={(value) => forceUpdate(group.set('description', value))}
                        />
                    </FormControl>

                    <Heading as="h3" size="md" mt={8}>
                        {t('createGroup.fields.section.registration').toString()}
                    </Heading>

                    <FormControl id="isOpen">
                        <Checkbox
                            borderColor={border}
                            defaultChecked={group.isOpen}
                            onChange={(event) => forceUpdate(group.set('isOpen', event.target.checked))}
                        >
                            {t('createGroup.fields.isOpen').toString()}
                        </Checkbox>
                    </FormControl>

                    <FormControl id="deadline">
                        <FormLabel>{t('createGroup.fields.deadline').toString()}</FormLabel>
                        <Input
                            type="datetime-local"
                            borderColor={border}
                            bg={bg}
                            placeholder={t('createGroup.fields.deadline').toString()}
                            defaultValue={moment(group.deadline).format('YYYY-MM-DD HH:mm:ss')}
                            onChange={(event) =>
                                forceUpdate(group.set('deadline', moment(event.target.value).toISOString()))
                            }
                            maxW={96}
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
                                    ? t('createGroup.button.update').toString()
                                    : t('createGroup.button.create').toString()}
                            </Button>
                        </HStack>
                    )}
                </Stack>
            )}
        </SendAPI>
    );
}
