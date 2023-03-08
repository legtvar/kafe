import { Box, BoxProps, FormHelperText, Input, InputGroup, Tag, TagCloseButton, TagLabel } from '@chakra-ui/react';
import { t } from 'i18next';
import { forAnimationFrame } from 'waitasecond';

interface ITagInputProps extends BoxProps {
    placeholder: string;
    tags: string[];
    setTags: (tags: string[]) => void;
    value: string;
    setValue: (value: string) => void;
}

export function TagInput({ placeholder, tags, setTags, value, setValue, ...rest }: ITagInputProps) {
    const addRole = async (tag: string) => {
        if (tag.length === 0) return;
        setTags([...tags, tag]);
        await forAnimationFrame();
        setValue('');
    };

    const deleteRole = (id: number) => {
        setTags(tags.filter((tag, i) => i !== id));
    };

    return (
        <Box {...rest}>
            <Box mb={1}>
                {tags.map((tag, i) => (
                    <Tag variant="solid" key={i} mr={2} mb={2}>
                        <TagLabel>{tag}</TagLabel>
                        <TagCloseButton onClick={() => deleteRole(i)} />
                    </Tag>
                ))}
            </Box>
            <InputGroup>
                <Input
                    placeholder={placeholder}
                    value={value}
                    onChange={(event) => setValue(event.target.value)}
                    onKeyDown={(event) =>
                        (event.key === 'Enter' || event.key === ',') &&
                        addRole((event.target as HTMLInputElement).value)
                    }
                />
            </InputGroup>
            <FormHelperText>{t('tagInput.note').toString()}</FormHelperText>
        </Box>
    );
}
