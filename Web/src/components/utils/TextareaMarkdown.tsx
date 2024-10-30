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

export function TextareaMarkdown({ value, defaultValue, ...props }: TextareaProps) {
    const [textareaValue, setTextareaValue] = useState<string>(value?.toString() || defaultValue?.toString() || '');

    return (
        <>
            <FormHelperText opacity={0.5}>{t('textareaMarkdown.allowMarkdown').toString()}</FormHelperText>
            <Textarea
                {...props}
                value={textareaValue}
                onChange={(event) => {
                    props.onChange && props.onChange(event);
                    setTextareaValue(event.target.value);
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
                            {textareaValue}
                        </Markdown>
                    </AccordionPanel>
                </AccordionItem>
            </Accordion>
        </>
    );
}
