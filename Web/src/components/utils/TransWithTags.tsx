import { Trans } from 'react-i18next';
import { Autolink } from './Autolink';
import { Autoemail } from './Autoemail';
import { ListItem, OrderedList, Text, UnorderedList } from '@chakra-ui/react';

interface ITransWithTags {
    i18nKey: string;
}

export function TransWithTags(props: ITransWithTags) {
    return (
        <Trans
            i18nKey={props.i18nKey}
            components={{
                a: <Autolink />,
                email: <Autoemail />,
                p: <Text mb={4} mt={4} textAlign="justify" />,
                h3: <Text mb={4} mt={8} fontSize="xx-large" fontWeight="semibold" as="h3" />,
                h4: <Text mb={4} mt={8} fontSize="x-large" fontWeight="semibold" as="h4" />,
                ul: <UnorderedList pl={4} mb={4}></UnorderedList>,
                ol: <OrderedList pl={4} mb={4}></OrderedList>,
                li: <ListItem pt={2} pb={2}></ListItem>,
            }}
        ></Trans>
    );
}
