import { FormHelperText, Text, Textarea, TextareaProps } from '@chakra-ui/react';
import { t } from 'i18next';
import { useState } from 'react';

interface ITextareaLimitedProps extends TextareaProps {
    min?: number;
    max?: number;
}

export function TextareaLimited({ min, max, onChange, value, defaultValue, ...rest }: ITextareaLimitedProps) {
    const [stateValue, setvalue] = useState(defaultValue || value || '');

    const tooLittle = min && (stateValue || '').toString().length < min;

    return (
        <>
            <Textarea
                onChange={(event) => {
                    let newvalue = event.target.value;
                    newvalue = newvalue.substring(0, max);

                    setvalue(newvalue);
                    if (onChange) {
                        onChange(event);
                    }
                }}
                value={stateValue}
                {...rest}
            />
            <FormHelperText>
                <>
                    {(stateValue || '').toString().length}/{max} {t('textarea.characters')}{' '}
                    {tooLittle && <Text as="span" color={'red.500'}>{`(${t('textarea.min')} ${min})`}</Text>}
                </>
            </FormHelperText>
        </>
    );
}
