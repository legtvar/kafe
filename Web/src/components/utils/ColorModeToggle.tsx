import { IconButton, IconButtonProps, useColorMode } from '@chakra-ui/react';
import { FiMoon, FiSun } from 'react-icons/fi';
import { useReload } from '../../hooks/useReload';

interface IColorModeToggleProps extends IconButtonProps {
    onColorModeToggled?: (lang: string) => void;
}

export function ColorModeToggle({ onColorModeToggled, ...rest }: IColorModeToggleProps) {
    const reload = useReload();
    const { colorMode, toggleColorMode } = useColorMode();

    return (
        <IconButton
            size="lg"
            variant="ghost"
            onClick={toggleColorMode}
            icon={colorMode === 'light' ? <FiMoon /> : <FiSun />}
            {...rest}
        />
    );
}
