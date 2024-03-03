import {
    Accordion,
    AccordionButton,
    AccordionIcon,
    AccordionItem,
    AccordionPanel,
    Box,
    ListItem,
    Text,
    UnorderedList,
} from '@chakra-ui/react';
import { t } from 'i18next';
import { TransWithTags } from '../../../utils/TransWithTags';

interface IIntroTextProps {
    displayDetails?: boolean;
}

export function IntroText(props: IIntroTextProps) {
    return (
        <Box maxWidth="70rem">
            <TransWithTags i18nKey="homeFestival.general" />

            {props.displayDetails && <TransWithTags i18nKey="homeFestival.details"></TransWithTags>}
        </Box>
    );
}
