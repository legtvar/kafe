import { Flex, FlexProps, Icon, useColorModeValue } from '@chakra-ui/react';
import { SelectableIcon } from '../../../routes';

export interface INavItemProps extends FlexProps {
    icon?: SelectableIcon;
    children: React.ReactNode;
    small?: true;
    selected?: boolean;
}
export const NavItem = ({ icon, children, small, selected, ...rest }: INavItemProps) => {
    return (
        <Flex
            align="center"
            py={small ? 2 : 4}
            px={small ? 3 : 4}
            mx={small ? 3 : 4}
            borderRadius="lg"
            role="group"
            cursor="pointer"
            fontWeight={selected ? 'bold' : undefined}
            _hover={{
                bg: useColorModeValue('brand.400', 'brand.700'),
                color: 'white',
            }}
            transition="0.1s ease"
            {...rest}
        >
            {icon && (
                <Icon
                    mr="4"
                    fontSize="16"
                    _groupHover={{
                        color: 'white',
                    }}
                    transition="0.1s ease"
                    as={(selected && icon.selected) || icon.default}
                />
            )}
            {children}
        </Flex>
    );
};
