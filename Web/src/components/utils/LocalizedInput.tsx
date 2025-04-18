import {
    Center,
    ComponentWithAs,
    FormControl,
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
    VStack,
} from '@chakra-ui/react';
import CountryLanguage from '@ladjs/country-language';
import * as Flags from 'country-flag-icons/react/3x2';
import i18next, { t } from 'i18next';
import { ChangeEvent, useMemo, useState } from 'react';
import { IoAdd, IoEarth, IoTrash } from 'react-icons/io5';
import { localizedString } from '../../schemas/generic';
import { getPrefered } from '../../utils/preferedLanguage';

export interface ILocalizedInputProps extends Omit<InputProps, 'value' | 'onChange'> {
    value: localizedString;
    onChange?: (value: localizedString) => void;
}

// Provide Chakra's AS functionality to the component
export const LocalizedInput: ComponentWithAs<'input', ILocalizedInputProps> = ({
    value,
    onChange,
    name,
    placeholder,
    type,
    as,
    ...rest
}: ILocalizedInputProps) => {
    const definedValue = value || { iv: '' };
    const [tabIndex, setTabIndex] = useState(0);

    // Make "iv" the first key
    const keys = Object.keys(definedValue).sort((a, b) => (a === 'iv' ? -1 : b === 'iv' ? 1 : 0));

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

    return (
        <Tabs variant="enclosed" onChange={(index) => setTabIndex(index)} index={tabIndex}>
            <TabList borderBottom="none" px={0}>
                {keys.map((key) => {
                    if (key === 'iv') {
                        return (
                            <Tab key={key} p={3}>
                                <IoEarth />
                            </Tab>
                        );
                    }

                    const lang = new Intl.Locale(key).maximize();
                    const CurrentFlag = flags[lang.region || ''];

                    return (
                        <Tab key={key} p={3}>
                            <CurrentFlag width={20} />
                        </Tab>
                    );
                })}

                <Menu isLazy>
                    <MenuButton as={IconButton} icon={<IoAdd />} variant="link" aria-label="Add" />
                    <MenuList overflowY="auto" maxH="20vh">
                        {translated
                            .filter((lang) => !Object.keys(definedValue).includes(lang.id))
                            .map((lang, key) => (
                                <MenuItem
                                    onClick={() => {
                                        definedValue[lang.id] = '';
                                        onChange && onChange(definedValue);
                                        setTabIndex(Object.keys(definedValue).length - 1);
                                    }}
                                    key={key}
                                >
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
                            .filter((lang) => !Object.keys(definedValue).includes(lang.id))
                            .map((lang, key) => (
                                <MenuItem
                                    onClick={() => {
                                        definedValue[lang.id] = '';
                                        onChange && onChange(definedValue);
                                        setTabIndex(Object.keys(definedValue).length - 1);
                                    }}
                                    key={key}
                                >
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
            </TabList>
            <TabPanels>
                {keys.map((key) => {
                    let langName = t('common.invariant');
                    if (key !== 'iv') {
                        const lang = new Intl.Locale(key).maximize();
                        langName =
                            CountryLanguage.getLanguage(lang.language).nativeName[0] ||
                            CountryLanguage.getLanguage(lang.language).name[0];
                    }

                    const inputProps: InputProps = {
                        placeholder: placeholder ? `${placeholder} - ${langName}` : undefined,
                        defaultValue: getPrefered(value, key),
                        onChange: (event: ChangeEvent<HTMLInputElement>) => {
                            definedValue[key] = event.target.value;
                            onChange && onChange(definedValue);
                        },
                        borderTopLeftRadius: 0,
                        type: type || 'text',
                        ...rest,
                    };

                    const Component = as || Input;

                    return (
                        <TabPanel key={key} p={0}>
                            <FormControl id={`${name}.${key}`}>
                                <HStack alignItems="start">
                                    <VStack flexGrow={1} alignItems="start" spacing={0}>
                                        <Component {...inputProps} />
                                    </VStack>
                                    <IconButton
                                        aria-label="Remove"
                                        variant="outline"
                                        icon={<IoTrash />}
                                        isDisabled={key === 'iv'}
                                        onClick={() => {
                                            setTabIndex(tabIndex - 1);
                                            delete definedValue[key];
                                            onChange && onChange(definedValue);
                                        }}
                                        colorScheme="red"
                                    />
                                </HStack>
                            </FormControl>
                        </TabPanel>
                    );
                })}
            </TabPanels>
        </Tabs>
    );
};
