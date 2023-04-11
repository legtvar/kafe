import { BoxProps, Button, HStack, IconButton, Menu, MenuButton, MenuItem, MenuList, VStack } from '@chakra-ui/react';
import { useState } from 'react';
import { AiOutlineCheck } from 'react-icons/ai';
import { BsDownload, BsGear } from 'react-icons/bs';
import ReactPlayer from 'react-player';
import { capitalize } from '../../../utils/capitalize';

interface IVideoProps extends BoxProps {
    sources: { [key: string]: string };
}

export function Video({ sources, ...rest }: IVideoProps) {
    const [quality, setQuality] = useState(Object.keys(sources)[0]);

    return (
        <VStack {...rest} role="group" spacing={4}>
            <ReactPlayer
                width={'100%'}
                height={'calc(100% - 72px)'}
                controls
                url={sources[quality]}
                onError={(error) => console.log(error)}
            />
            <HStack w="100%" px={5}>
                <Menu>
                    <MenuButton as={Button} aria-label="Quality" leftIcon={<BsGear />}>
                        {capitalize(quality)}
                    </MenuButton>
                    <MenuList>
                        {Object.keys(sources).map((source, key) => (
                            <MenuItem
                                key={key}
                                value={source}
                                icon={source === quality ? <AiOutlineCheck /> : undefined}
                                onClick={() => setQuality(source)}
                            >
                                {capitalize(source)}
                            </MenuItem>
                        ))}
                    </MenuList>
                </Menu>
                <Menu>
                    <MenuButton as={IconButton} aria-label="Download" icon={<BsDownload />} />
                    <MenuList>
                        {Object.keys(sources).map((source, key) => (
                            <MenuItem key={key} as={'a'} value={source} href={`${sources[source]}`} target="_blank">
                                {capitalize(source)}
                            </MenuItem>
                        ))}
                    </MenuList>
                </Menu>
            </HStack>
        </VStack>
    );
}
