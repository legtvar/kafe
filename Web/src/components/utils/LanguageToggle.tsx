import {
    Center,
    HStack,
    IconButton,
    IconButtonProps,
    Menu,
    MenuButton,
    MenuDivider,
    MenuItemOption,
    MenuList,
    MenuOptionGroup,
    Text,
} from '@chakra-ui/react';
import CountryLanguage from '@ladjs/country-language';
import * as Flags from 'country-flag-icons/react/3x2';
import i18next, { t } from 'i18next';
import { BsCopy } from 'react-icons/bs';
import { LS_LANGUAGE_APP_KEY } from '../../languageConfig';
import { getRawPrefered, LS_LANGUAGE_CONTENT_KEY, setPreferedLanguage } from '../../utils/preferedLanguage';

interface ILanguageToggleProps extends IconButtonProps {
    onLanguageToggled?: (lang: string) => void;
}

export function LanguageToggle({ onLanguageToggled: onLanguageChange, ...rest }: ILanguageToggleProps) {
    const changeLanguage = (lang: string) => {
        i18next.changeLanguage(lang);
        localStorage.setItem(LS_LANGUAGE_APP_KEY, lang);
        onLanguageChange && onLanguageChange(lang);
    };

    const changeContentLanguage = (lang: string) => {
        setPreferedLanguage(lang);
        localStorage.setItem(LS_LANGUAGE_CONTENT_KEY, lang);
        onLanguageChange && onLanguageChange(lang);
    };

    const translated = Object.keys(i18next.options.resources!).map((lang) => new Intl.Locale(lang).maximize());

    const flags = Flags as any as Record<string, Flags.FlagComponent>;

    const all = CountryLanguage.getLanguages()
        .map((lang) => lang.iso639_1)
        .filter((lang) => lang.length > 0)
        .map((lang) => new Intl.Locale(lang).maximize());

    const mapper = (onClick: (lang: Intl.Locale) => void) => (lang: Intl.Locale) => {
        const Flag: Flags.FlagComponent = flags[lang.region || ''];
        if (!Flag) {
            return null;
        }
        return (
            <MenuItemOption value={lang.language} onClick={() => onClick(lang)}>
                <HStack>
                    <Flag width={18} />
                    <Text>
                        {CountryLanguage.getLanguage(lang.language).nativeName[0] ||
                            CountryLanguage.getLanguage(lang.language).name[0]}
                    </Text>
                </HStack>
            </MenuItemOption>
        );
    };

    const CurrentFlag = flags[new Intl.Locale(i18next.language).maximize().region || ''];

    return (
        <Menu closeOnSelect={false}>
            <MenuButton
                as={IconButton}
                size="lg"
                icon={<CurrentFlag />}
                p={3}
                variant="ghost"
                aria-label="Change language"
            />
            <MenuList overflowY="auto" maxH="80vh">
                <MenuOptionGroup title={t('languageToggle.system')} value={i18next.language}>
                    {translated.map(
                        mapper((lang) => {
                            changeLanguage(lang.language);
                        }),
                    )}
                </MenuOptionGroup>
                <MenuDivider />
                <MenuOptionGroup title={t('languageToggle.content')} value={getRawPrefered()}>
                    <MenuItemOption
                        value={'_inherit'}
                        onClick={() => {
                            changeContentLanguage('_inherit');
                        }}
                    >
                        <HStack>
                            <Center w="18px">
                                <BsCopy />
                            </Center>
                            <Text>{t('languageToggle.matching')}</Text>
                        </HStack>
                    </MenuItemOption>
                    {all.map(
                        mapper((lang) => {
                            changeContentLanguage(lang.language);
                        }),
                    )}
                </MenuOptionGroup>
            </MenuList>
        </Menu>
    );
}
