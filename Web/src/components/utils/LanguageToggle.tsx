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
    useForceUpdate,
} from '@chakra-ui/react';
import CountryLanguage from '@ladjs/country-language';
import * as Flags from 'country-flag-icons/react/3x2';
import i18next, { t } from 'i18next';
import { useMemo } from 'react';
import { BsChevronDown, BsCopy } from 'react-icons/bs';
import { LS_LANGUAGE_APP_KEY } from '../../languageConfig';
import {
    getRawPrefered,
    LS_LANGUAGE_CONTENT_KEY,
    preferedLanguage,
    setPreferedLanguage,
} from '../../utils/preferedLanguage';

interface ILanguageToggleProps extends IconButtonProps {
    onLanguageToggled?: (lang: string) => void;
}

export function LanguageToggle({ onLanguageToggled: onLanguageChange, ...rest }: ILanguageToggleProps) {
    const forceReload = useForceUpdate();

    const changeLanguage = (lang: string) => {
        i18next.changeLanguage(lang);
        localStorage.setItem(LS_LANGUAGE_APP_KEY, lang);
        onLanguageChange && onLanguageChange(lang);
        forceReload();
    };

    const changeContentLanguage = (lang: string) => {
        setPreferedLanguage(lang);
        localStorage.setItem(LS_LANGUAGE_CONTENT_KEY, lang);
        onLanguageChange && onLanguageChange(lang);
        forceReload();
    };

    const translated = useMemo(
        () => Object.keys(i18next.options.resources!).map((lang) => new Intl.Locale(lang).maximize()),
        [],
    );

    const flags = useMemo(() => Flags as any as Record<string, Flags.FlagComponent>, []);

    const all = useMemo(
        () =>
            CountryLanguage.getLanguages()
                .map((lang) => lang.iso639_1)
                .filter((lang) => lang.length > 0)
                .map((lang) => new Intl.Locale(lang).maximize()),
        [],
    );

    const mapper = (onClick: (lang: Intl.Locale) => void) => (lang: Intl.Locale, key: number) => {
        const Flag: Flags.FlagComponent = flags[lang.region || ''];
        if (!Flag) {
            return null;
        }

        const lng = CountryLanguage.getLanguage(lang.language);

        return (
            <MenuItemOption key={key} value={lang.language} onClick={() => onClick(lang)}>
                <HStack>
                    <Flag width={18} />
                    <Text>
                        {lng.name[0]} {lng.nativeName[0] && `(${lng.nativeName[0]})`}
                    </Text>
                </HStack>
            </MenuItemOption>
        );
    };

    const CurrentFlag = flags[new Intl.Locale(i18next.language).maximize().region || ''];

    return (
        <Menu closeOnSelect={false}>
            <MenuButton as={IconButton} size="lg" icon={<CurrentFlag />} p={3} variant="ghost" {...rest} />
            <MenuList overflowY="auto" maxH="80vh" w={96}>
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
                    {getRawPrefered() === '_inherit' ? (
                        <MenuItemOption
                            onClick={() => {
                                changeContentLanguage(preferedLanguage());
                            }}
                        >
                            <HStack>
                                <Center w="18px">
                                    <BsChevronDown />
                                </Center>
                                <Text>{t('languageToggle.more')}</Text>
                            </HStack>
                        </MenuItemOption>
                    ) : (
                        all.map(
                            mapper((lang) => {
                                changeContentLanguage(lang.language);
                            }),
                        )
                    )}
                </MenuOptionGroup>
            </MenuList>
        </Menu>
    );
}
