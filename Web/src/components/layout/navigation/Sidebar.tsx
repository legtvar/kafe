import { Box, BoxProps, CloseButton, Flex, useColorModeValue } from '@chakra-ui/react';
import { useTranslation } from 'react-i18next';
import { IoReader, IoReaderOutline } from 'react-icons/io5';
import { Link, useMatches } from 'react-router-dom';
import { AppRoute, authRoutes } from '../../../routes';
import { Footer } from '../Footer';
import { Logo } from '../Logo';
import { NavItem } from './NavItem';

interface ISidebarProps extends BoxProps {
    onClose: () => void;
}

export function Sidebar({ onClose, ...rest }: ISidebarProps) {
    const matches = useMatches();
    const i18next = useTranslation();
    const fgColor = useColorModeValue('gray.900', 'white');

    const routeValues = authRoutes(i18next.t);

    const match = matches[matches.length - 1].id
        .split('-')
        .slice(2)
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
                <>
                    <Link to={'/auth' + fullPath}>
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
                </>
            );
        };

    const items = routeValues.filter((route) => route.inMenu).map(mapper('', false, match.path));
    return (
        <Box
            bg={useColorModeValue('white', 'gray.900')}
            borderRight="1px"
            borderRightColor={useColorModeValue('gray.200', 'gray.700')}
            w={{ base: 'full', md: 64 }}
            pos="fixed"
            h="full"
            overflowY="auto"
            {...rest}
        >
            <Flex direction="column" minH="100%">
                <Flex h="20" alignItems="center" mx="8" justifyContent="space-between" key="heading">
                    <Link to="/">
                        <Logo />
                    </Link>
                    <CloseButton display={{ base: 'flex', md: 'none' }} onClick={onClose} />
                </Flex>
                {items}
                <Footer mt="auto" key="footer" />
            </Flex>
        </Box>
    );
}
