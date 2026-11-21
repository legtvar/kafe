import {
    Center,
    ComponentWithAs,
    FormControl,
    FormHelperText,
    HStack,
    IconButton,
    Input,
    InputProps,
    Menu,
    MenuButton,
    MenuDivider,
    MenuItem,
    MenuList,
    Tab,
    TabList,
    TabPanel,
    TabPanels,
    Tabs,
    Text,
    Tooltip,
    useColorModeValue,
    VStack,
} from '@chakra-ui/react';
import CountryLanguage from '@ladjs/country-language';
import * as Flags from 'country-flag-icons/react/3x2';
import i18next, { t } from 'i18next';
import { ChangeEvent, useMemo, useState } from 'react';
import { IoAdd, IoEarth, IoTrash } from 'react-icons/io5';
import { localizedString } from '../../schemas/generic';

export type LocalizedInputRequirement = string | string[];

export interface ILocalizedInputProps extends Omit<InputProps, 'value' | 'onChange' | 'required'> {
    value: localizedString;
    onChange?: (value: localizedString) => void;
    required?: LocalizedInputRequirement[];
    split?: boolean;
    minLength?: number;
    maxLength?: number;
}

// Provide Chakra's AS functionality to the component
export const LocalizedInput: ComponentWithAs<'input', ILocalizedInputProps> = ({
    value,
    onChange,
    name,
    placeholder,
    type,
    as,
    required = [],
    split = false,
    minLength,
    maxLength,
    ...rest
}: ILocalizedInputProps) => {
    const localizedValue: localizedString = { iv: '', ...(value || {}) };

    // Required languages must be present even before they have been edited.
    required.forEach((requirement) => {
        const languages = Array.isArray(requirement) ? requirement : [requirement];
        languages.forEach((language) => {
            if (!(language in localizedValue)) {
                localizedValue[language] = '';
            }
        });
    });

    const [tabIndex, setTabIndex] = useState(0);

    // Make "iv" the first key
    const keys = Object.keys(localizedValue).sort((a, b) => (a === 'iv' ? -1 : b === 'iv' ? 1 : 0));
    // NB: "important" keys are those that occur in `required`
    const importantKeys = required.flatMap((r) => (Array.isArray(r) ? r : [r]));

    const flags = useMemo(() => Flags as any as Record<string, Flags.FlagComponent>, []);
    const allFlags = useMemo(
        () =>
            CountryLanguage.getLanguages()
                .map((lang) => lang.iso639_1)
                .filter((lang) => lang.length > 0)
                .map((lang) => new Intl.Locale(lang).maximize())
                .map((lang) => {
                    const lng = CountryLanguage.getLanguage(lang.language);

                    return {
                        id: lang.language,
                        name: `${lng.name[0]} ${lng.nativeName[0] ? `(${lng.nativeName[0]})` : ''}`,
                        flag: flags[lang.region || ''],
                    };
                })
                .filter((lang) => lang.flag),
        [],
    );

    const translated = useMemo(
        () =>
            Object.keys(i18next.options.resources!)
                .map((lang) => new Intl.Locale(lang).maximize())
                .map((lang) => {
                    const lng = CountryLanguage.getLanguage(lang.language);

                    return {
                        id: lang.language,
                        name: `${lng.name[0]} ${lng.nativeName[0] ? `(${lng.nativeName[0]})` : ''}`,
                        flag: flags[lang.region || ''],
                    };
                }),
        [],
    );

    const getLanguageName = (key: string) => {
        if (key === 'iv') {
            return t('common.invariant');
        }

        const lang = new Intl.Locale(key).maximize();
        return (
            CountryLanguage.getLanguage(lang.language).nativeName[0] ||
            CountryLanguage.getLanguage(lang.language).name[0]
        );
    };

    const getLanguageHint = (key: string) => {
        if (key === 'iv') {
            return t('common.invariantHint');
        }

        return getLanguageName(key);
    };

    const languageIcon = (key: string) => {
        if (key === 'iv') {
            return <IoEarth />;
        }

        const lang = new Intl.Locale(key).maximize();
        const CurrentFlag = flags[lang.region || ''];
        return CurrentFlag ? <CurrentFlag width={20} /> : <Text>{key.toUpperCase()}</Text>;
    };

    const addLanguage = (language: string) => {
        onChange?.({ ...localizedValue, [language]: '' });
        setTabIndex(keys.length);
    };

    const addLanguageMenu = (
        <Menu isLazy>
            <MenuButton
                as={IconButton}
                icon={<IoAdd />}
                size="lg"
                variant="link"
                aria-label="Add"
                height="2rem"
                borderWidth='1px'
                borderColor={useColorModeValue('gray.300', 'gray.600')}
                _hover={{
                    bg: useColorModeValue('gray.50', 'gray.950'),
                }}
            />
            <MenuList overflowY="auto" maxH="20vh">
                {translated
                    .filter((lang) => !Object.keys(localizedValue).includes(lang.id))
                    .map((lang, key) => (
                        <MenuItem onClick={() => addLanguage(lang.id)} key={key}>
                            <HStack>
                                <Center w="18px">
                                    <lang.flag />
                                </Center>
                                <Text>{lang.name}</Text>
                            </HStack>
                        </MenuItem>
                    ))}
                <MenuDivider />
                {allFlags
                    .filter((lang) => !Object.keys(localizedValue).includes(lang.id))
                    .map((lang, key) => (
                        <MenuItem onClick={() => addLanguage(lang.id)} key={key}>
                            <HStack>
                                <Center w="18px">
                                    <lang.flag />
                                </Center>
                                <Text>{lang.name}</Text>
                            </HStack>
                        </MenuItem>
                    ))}
            </MenuList>
        </Menu>
    );

    const renderInput = (key: string) => {
        const langName = getLanguageName(key);
        const alternatives = required.find((r): r is string[] => Array.isArray(r) && r.includes(key)) ?? [];
        const hasAlternativeAlready = alternatives.some((lang) => (localizedValue[lang] ?? '').length > 0);
        const isRequired = required.includes(key) || (alternatives.length > 0 && !hasAlternativeAlready);

        const inputProps: InputProps = {
            placeholder: placeholder ? `${placeholder} - ${langName}` : undefined,
            value: localizedValue[key],
            onChange: (event: ChangeEvent<HTMLInputElement>) => {
                onChange?.({ ...localizedValue, [key]: event.target.value });
            },
            borderTopLeftRadius: split ? undefined : 0,
            borderLeftRadius: split ? 0 : undefined,
            type: type || 'text',
            isRequired,
            ...rest,
        };

        const valueLength = (inputProps.value || '').toString().length;
        const tooShort = minLength && valueLength < minLength && !(!isRequired && valueLength === 0);
        const tooLong = maxLength && valueLength > maxLength;

        const Component = as || Input;

        return (
            <FormControl id={`${name}.${key}`} isRequired={isRequired} w="100%">
                <VStack gap={0}>
                    <HStack alignItems="stretch" gap={0} w="100%">
                        {split && (
                            <Tooltip label={getLanguageHint(key)} placement="auto" hasArrow>
                                <Center
                                    minW="2.5rem"
                                    aria-label={langName}
                                    bg={useColorModeValue('gray.100', 'gray.900')}
                                    borderColor={useColorModeValue('gray.300', 'gray.700')}
                                    borderRightWidth="0px"
                                    borderWidth="1px"
                                    borderLeftRadius="md"
                                >
                                    {languageIcon(key)}
                                </Center>
                            </Tooltip>
                        )}
                        <VStack flexGrow={1} alignItems="start" spacing={0}>
                            <Component {...inputProps} />
                        </VStack>
                        <IconButton
                            aria-label="Remove"
                            variant="outline"
                            ml={2}
                            icon={<IoTrash />}
                            isDisabled={key === 'iv' || importantKeys.includes(key)}
                            onClick={() => {
                                setTabIndex(Math.max(0, tabIndex - 1));
                                const remaining = { ...localizedValue };
                                delete remaining[key];
                                onChange?.(remaining);
                            }}
                            colorScheme="red"
                        />
                    </HStack>
                    <FormHelperText alignSelf="flex-start">
                        <>
                            <Text as="span" color={tooLong ? 'red.500' : 'inherit'}>
                                {(inputProps.value || '').toString().length}
                                {maxLength !== undefined && `/${maxLength}`} {t('textarea.characters')}{' '}
                            </Text>

                            {tooShort && (
                                <Text as="span" color={'red.500'}>{`(${t('textarea.min')} ${minLength})`}</Text>
                            )}
                        </>
                    </FormHelperText>
                </VStack>
            </FormControl>
        );
    };

    if (split) {
        return (
            <VStack alignItems="stretch" spacing={4}>
                {keys.map((key) => (
                    <HStack key={key} alignItems="start">
                        {renderInput(key)}
                    </HStack>
                ))}
                {addLanguageMenu}
            </VStack>
        );
    }

    return (
        <Tabs variant="enclosed" onChange={(index) => setTabIndex(index)} index={tabIndex}>
            <TabList borderBottom="none" px={0}>
                {keys.map((key) => (
                    <Tooltip key={key} label={getLanguageHint(key)} hasArrow>
                        <Tab
                            p={3}
                            aria-label={getLanguageName(key)}
                            _selected={{
                                bg: useColorModeValue('white', 'black'),
                                borderColor: useColorModeValue('gray.300', 'gray.600'),
                            }}
                            _hover={{
                                bg: useColorModeValue('gray.50', 'gray.950'),
                                borderColor: useColorModeValue('gray.300', 'gray.600'),
                            }}
                        >
                            {languageIcon(key)}
                        </Tab>
                    </Tooltip>
                ))}
                {addLanguageMenu}
            </TabList>
            <TabPanels>
                {keys.map((key) => (
                    <TabPanel key={key} p={0} tabIndex={-1}>
                        {renderInput(key)}
                    </TabPanel>
                ))}
            </TabPanels>
        </Tabs>
    );
};
