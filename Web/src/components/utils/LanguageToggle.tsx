import { IconButton, IconButtonProps } from '@chakra-ui/react';
import { CZ, GB } from 'country-flag-icons/react/3x2';
import i18next from 'i18next';
import { useReload } from '../../hooks/useReload';

interface ILanguageToggleProps extends IconButtonProps {
    onLanguageToggled?: (lang: string) => void;
}

export function LanguageToggle({ onLanguageToggled: onLanguageChange, ...rest }: ILanguageToggleProps) {
    const reload = useReload();

    const changeLanguage = (lang: string) => {
        i18next.changeLanguage(lang);
        reload();
        onLanguageChange && onLanguageChange(lang);
    };

    switch (i18next.language) {
        case 'en':
            return (
                <IconButton
                    size="lg"
                    variant="ghost"
                    icon={<CZ />}
                    p={3}
                    onClick={() => changeLanguage('cs')}
                    {...rest}
                />
            );

        case 'cs':
            return (
                <IconButton
                    size="lg"
                    variant="ghost"
                    icon={<GB />}
                    p={3}
                    onClick={() => changeLanguage('en')}
                    {...rest}
                />
            );
    }

    // This should not happen :D
    return <IconButton size="lg" variant="ghost" icon={<CZ />} p={3} onClick={() => changeLanguage('cs')} {...rest} />;
}
