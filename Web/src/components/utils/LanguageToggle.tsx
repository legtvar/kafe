import { IconButton } from '@chakra-ui/react';
import { CZ, GB } from 'country-flag-icons/react/3x2';
import i18next from 'i18next';
import { useReload } from '../../hooks/useReload';

interface ILanguageToggleProps {
    onChange?: (lang: string) => void;
}

export function LanguageToggle(props: ILanguageToggleProps) {
    const reload = useReload();

    const changeLanguage = (lang: string) => {
        i18next.changeLanguage(lang);
        reload();
        props.onChange && props.onChange(lang);
    };

    switch (i18next.language) {
        case 'en':
            return (
                <IconButton
                    size="lg"
                    variant="ghost"
                    aria-label="Language"
                    icon={<CZ />}
                    p={3}
                    onClick={() => changeLanguage('cs')}
                />
            );

        case 'cs':
            return (
                <IconButton
                    size="lg"
                    variant="ghost"
                    aria-label="Language"
                    icon={<GB />}
                    p={3}
                    onClick={() => changeLanguage('en')}
                />
            );
    }

    // This should not happen :D
    return (
        <IconButton
            size="lg"
            variant="ghost"
            aria-label="Language"
            icon={<CZ />}
            p={3}
            onClick={() => changeLanguage('cs')}
        />
    );
}
