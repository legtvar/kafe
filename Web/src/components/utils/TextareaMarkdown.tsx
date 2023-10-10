import {
    Accordion,
    AccordionButton,
    AccordionIcon,
    AccordionItem,
    AccordionPanel,
    Box,
    FormHelperText,
    Textarea,
    TextareaProps,
} from '@chakra-ui/react';
import ChakraUIRenderer from 'chakra-ui-markdown-renderer';
import { t } from 'i18next';
import { useState } from 'react';
import Markdown from 'react-markdown';

export function TextareaMarkdown(props: TextareaProps) {
    const [value, setValue] = useState<string>(props.value?.toString() || props.defaultValue?.toString() || '');

    return (
        <>
            <FormHelperText opacity={0.5}>{t('textareaMarkdown.allowMarkdown').toString()}</FormHelperText>
            <Textarea
                {...props}
                value={value}
                onChange={(event) => {
                    props.onChange && props.onChange(event);
                    setValue(event.target.value);
                }}
            />

            <Accordion allowToggle>
                <AccordionItem>
                    <AccordionButton>
                        <Box as="span" flex="1" textAlign="left">
                            {t('textareaMarkdown.preview').toString()}
                        </Box>
                        <AccordionIcon />
                    </AccordionButton>
                    <AccordionPanel pb={4}>
                        <Markdown components={ChakraUIRenderer()} skipHtml>
                            {value}
                        </Markdown>
                    </AccordionPanel>
                </AccordionItem>
            </Accordion>
        </>
    );
}
