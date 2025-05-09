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
import { t } from 'i18next';
import { useState } from 'react';
import { ChakraMarkdown } from './ChakraMarkdown';

export function TextareaMarkdown({ value, defaultValue, ...props }: TextareaProps) {
    const [textareaValue, setTextareaValue] = useState<string>(value?.toString() || defaultValue?.toString() || '');

    return (
        <>
            <Textarea
                {...props}
                value={textareaValue}
                onChange={(event) => {
                    props.onChange && props.onChange(event);
                    setTextareaValue(event.target.value);
                }}
            />

            <FormHelperText opacity={0.5} mb={2}>
                {t('textareaMarkdown.allowMarkdown').toString()}
            </FormHelperText>

            <Accordion allowToggle w="full">
                <AccordionItem>
                    <AccordionButton>
                        <Box as="span" flex="1" textAlign="left">
                            {t('textareaMarkdown.preview').toString()}
                        </Box>
                        <AccordionIcon />
                    </AccordionButton>
                    <AccordionPanel pb={4}>
                        <ChakraMarkdown>{textareaValue}</ChakraMarkdown>
                    </AccordionPanel>
                </AccordionItem>
            </Accordion>
        </>
    );
}
