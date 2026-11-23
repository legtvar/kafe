import { FormControl, FormLabel, Input, Textarea } from '@chakra-ui/react';
import { t } from 'i18next';
import useForceUpdate from 'use-force-update';
import { Author } from '../../../data/Author';
import { LocalizedInput } from '../LocalizedInput';

interface IAuthorInfoFormProps {
    author: Author;
    onChange: (author: Author) => void;
}

export function AuthorInfoForm({ author, onChange }: IAuthorInfoFormProps) {
    const forceUpdate = useForceUpdate();

    function update<K extends keyof Author>(key: K, value: Author[K]) {
        onChange(author.set(key, value));
        forceUpdate();
    }

    return (
        <>
            <FormControl mb={6}>
                <FormLabel>{t('authorSelect.name').toString()}</FormLabel>
                <Input
                    placeholder={t('authorSelect.name').toString()}
                    value={author.name}
                    onChange={(event) => update('name', event.target.value)}
                />
            </FormControl>

            <FormControl mb={6}>
                <FormLabel>{t('authorSelect.uco').toString()}</FormLabel>
                <Input
                    placeholder={t('authorSelect.uco').toString()}
                    value={author.uco}
                    onChange={(event) => update('uco', event.target.value)}
                />
            </FormControl>

            <FormControl mb={6}>
                <FormLabel>{t('authorSelect.email').toString()}</FormLabel>
                <Input
                    placeholder={t('authorSelect.email').toString()}
                    value={author.email}
                    onChange={(event) => update('email', event.target.value)}
                />
            </FormControl>

            <FormControl mb={6}>
                <FormLabel>{t('authorSelect.phone').toString()}</FormLabel>
                <Input
                    placeholder={t('authorSelect.phone').toString()}
                    value={author.phone}
                    onChange={(event) => update('phone', event.target.value)}
                />
            </FormControl>

            <FormControl>
                <FormLabel>{t('authorSelect.bio').toString()}</FormLabel>
                <LocalizedInput
                    as={Textarea}
                    minLength={0}
                    maxLength={200}
                    name="bio"
                    placeholder={t('authorSelect.bio').toString()}
                    value={{ cs: '', en: '', ...author.bio }}
                    onChange={(value) => update('bio', value)}
                    split
                />
            </FormControl>
        </>
    );
}
