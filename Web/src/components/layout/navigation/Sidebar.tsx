import { BoxProps, CloseButton, Flex, useColorModeValue, VStack } from '@chakra-ui/react';
import { Fragment } from 'react';
import { useTranslation } from 'react-i18next';
import { IoReader, IoReaderOutline } from 'react-icons/io5';
import { Link, useMatches } from 'react-router-dom';
import { useAuth, useOrganizations } from '../../../hooks/Caffeine';
import { useAuthLink } from '../../../hooks/useAuthLink';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { AppRoute, authRoutes } from '../../../routes';
import { LS_LATEST_ORG_KEY } from '../../pages/root/OrganizationRedirect';
import { OrganizationAvatar } from '../../utils/OrganizationAvatar/OrganizationAvatar';
import { Footer } from '../Footer';
import { ReportButton } from '../ReportButton';
import { NavItem } from './NavItem';

interface ISidebarProps extends BoxProps {
    onClose: () => void;
    forceReload: () => void;
}

export const SIDEBAR_WIDTH = 80;

export function Sidebar({ onClose, forceReload, ...rest }: ISidebarProps) {
    const matches = useMatches();
    const i18next = useTranslation();
    const fgColor = useColorModeValue('gray.900', 'white');
    const { border, bg, bgDarker, color } = useColorScheme();

    const routeValues = authRoutes(i18next.t, useAuth().user, useOrganizations().currentOrganization);

    const match = matches[matches.length - 1].id
        .split('-')
        .slice(3)
        .reduce(
            ({ route, path }, id) => ({
                route: route.children![parseInt(id)],
                path: path + '/' + route.children![parseInt(id)].path,
            }),
            { route: { children: routeValues, path: '' } as AppRoute, path: '' },
        );

    const mapper =
        (path: string, isChild: boolean, matchPath: string) =>
        (route: AppRoute, i: number): React.ReactNode => {
            const fullPath = path + '/' + route.path;
            const children = route.children?.filter((route) => route.inMenu).map(mapper(fullPath, true, matchPath));

            const matchPathStripped = matchPath.replaceAll('//', '/');
            const fullPathStripped = fullPath.replaceAll('//', '/');
            const selected =
                matchPathStripped.startsWith(fullPathStripped) &&
                (fullPathStripped.length > 1 || matchPathStripped === fullPathStripped); // Special case for the "Home" path

            return (
                <Fragment key={i}>
                    <Link to={useAuthLink(fullPath)}>
                        <NavItem
                            key={i}
                            icon={
                                !isChild
                                    ? route.icon || {
                                          default: IoReaderOutline,
                                          selected: IoReader,
                                      }
                                    : undefined
                            }
                            small={isChild || undefined}
                            selected={selected && (!children?.length || matchPathStripped === fullPathStripped)}
                        >
                            {route.title}
                        </NavItem>
                    </Link>
                    {selected && children && children.length > 0 && (
                        <Flex
                            direction="column"
                            position="relative"
                            ml={isChild ? '25px' : '39px'}
                            key={`${i}_sub`}
                            _before={{
                                display: 'block',
                                content: '""',
                                position: 'absolute',
                                top: 1,
                                left: 0,
                                bottom: 1,
                                width: '2px',
                                borderRadius: 'full',
                                backgroundColor: fgColor,
                            }}
                        >
                            {children}
                        </Flex>
                    )}
                </Fragment>
            );
        };

    const { organizations } = useOrganizations();

    const items = routeValues.filter((route) => route.inMenu).map(mapper('', false, match.path));
    return (
        <Flex
            flexDir="row"
            bg={useColorModeValue('white', 'gray.900')}
            borderRight="1px"
            borderRightColor={useColorModeValue('gray.200', 'gray.700')}
            w={{ base: 'full', md: SIDEBAR_WIDTH }}
            flexShrink={0}
            flexGrow={{ base: 1, md: 0 }}
            overflowY="auto"
            {...rest}
        >
            <VStack
                w={16}
                align="center"
                justify="start"
                py={6}
                spacing={6}
                bg={useColorModeValue('gray.100', 'gray.800')}
                borderRight="1px"
                borderRightColor={border}
            >
                {organizations.map((org, key) => (
                    <Link
                        to={useAuthLink(undefined, org.id)}
                        key={key}
                        onClick={() => {
                            localStorage.setItem(LS_LATEST_ORG_KEY, org.id);
                            forceReload();
                        }}
                    >
                        <OrganizationAvatar organization={org} size="sm" cursor="pointer" />
                    </Link>
                ))}
            </VStack>
            <Flex direction="column" minH="100%" flexGrow={1} flexShrink={1} justifyContent="space-between">
                <CloseButton display={{ base: 'flex', md: 'none' }} alignSelf="end" m={4} mb={-2} onClick={onClose} />
                <Flex direction="column" grow={1} mt={4}>
                    {items}
                </Flex>
                <ReportButton
                    alignSelf="stretch"
                    mx={4}
                    opacity={0.3}
                    _hover={{
                        opacity: 1,
                    }}
                    transition="opacity 0.2s linear"
                />
                <Footer key="footer" />
            </Flex>
        </Flex>
    );
}
