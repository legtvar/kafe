import { FormHelperText, Textarea, TextareaProps } from '@chakra-ui/react';
import { t } from 'i18next';
import { useState } from 'react';

interface ITextareaLimitedProps extends TextareaProps {
    limit: number;
}

export function TextareaLimited(props: ITextareaLimitedProps) {
    const [value, setvalue] = useState(props.value || '');

    return (
        <>
            <Textarea
                onChange={(event) => {
                    let newvalue = event.target.value;
                    newvalue = newvalue.substring(0, props.limit);

                    setvalue(newvalue);
                    if (props.onChange) {
                        props.onChange(event);
                    }
                }}
                value={value}
                {...props}
            />
            <FormHelperText>
                <>
                    {(value || '').toString().length}/{props.limit} {t('textarea.characters')}
                </>
            </FormHelperText>
        </>
    );
}
