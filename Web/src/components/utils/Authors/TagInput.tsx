import { Box, BoxProps, FormHelperText, Input, InputGroup, Tag, TagCloseButton, TagLabel } from '@chakra-ui/react';
import { t } from 'i18next';
import { useState } from 'react';
import { forAnimationFrame } from 'waitasecond';

interface ITagInputProps extends BoxProps {
    placeholder: string;
    tags: string[];
    setTags: (tags: string[]) => void;
}

export function TagInput({ placeholder, tags, setTags, ...rest }: ITagInputProps) {
    const [value, setvalue] = useState<string>('');

    const addRole = async (tag: string) => {
        if (tag.length === 0) return;
        setTags([...tags, tag]);
        await forAnimationFrame();
        setvalue('');
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
                    onChange={(event) => setvalue(event.target.value)}
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
