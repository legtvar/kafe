import { FormHelperText, Input, InputProps, Text } from '@chakra-ui/react';
import { t } from 'i18next';
import { useState } from 'react';

interface IInputLimitedProps extends InputProps {
    min?: number;
    max?: number;
}

export function InputLimited({ min, max, onChange, value, defaultValue, ...rest }: IInputLimitedProps) {
    const [stateValue, setValue] = useState(defaultValue || value || '');

    const tooLittle = min !== undefined && (stateValue || '').toString().length < min;

    return (
        <>
            <Input
                onChange={(event) => {
                    let newValue = event.target.value;
                    newValue = newValue.substring(0, max);

                    setValue(newValue);
                    if (onChange) {
                        onChange(event);
                    }
                }}
                value={stateValue}
                {...rest}
            />
            <FormHelperText>
                <>
                    {(stateValue || '').toString().length}{max !== undefined && `/${max}`} {t('textarea.characters')}{' '}
                    {tooLittle && <Text as="span" color={'red.500'}>{`(${t('textarea.min')} ${min})`}</Text>}
                </>
            </FormHelperText>
        </>
    );
}
