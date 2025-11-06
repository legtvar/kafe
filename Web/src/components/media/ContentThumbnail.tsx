import { Box, BoxProps, Center, HStack, Icon, useColorModeValue, useDisclosure, VStack } from '@chakra-ui/react';
import { IoVideocam, IoWarning } from 'react-icons/io5';
import { SiBlender } from "react-icons/si";
import { Artifact } from '../../data/Artifact';
import { useApi } from '../../hooks/Caffeine';
import { ContentPopup } from './ContentPopup';

interface IContentThumbnailProps extends BoxProps {
    artifact: Artifact;
    passive?: boolean;
    onClick?: () => void;
}

type ContentType = 'Video' | 'Image' | 'Blend' | 'Unknown';

export function ContentThumbnail({ artifact, passive, onClick, ...props }: IContentThumbnailProps) {
    const api = useApi();
    const { isOpen, onOpen, onClose } = useDisclosure();

    let type: ContentType = 'Unknown';

    // Determine the type of the content
    if (artifact.shards.some((shard) => shard.kind === 'video')) {
        type = 'Video';
    } else if (artifact.shards.some((shard) => shard.kind === 'image')) {
        type = 'Image';
    } else if (artifact.shards.some((shard) => shard.kind === 'blend')) {
        type = 'Blend';
    }

    const onClickHandler = () => {
        if (onClick) {
            onClick();
        }

        if (!passive) {
            onOpen();
        }
    };

    return (
        <>
            <VStack
                maxW="100%"
                bg={useColorModeValue('gray.200', 'gray.900')}
                _hover={{
                    shadow: 'lg',
                }}
                borderColor={useColorModeValue('gray.300', 'gray.700')}
                borderWidth={1}
                borderStyle="solid"
                flexGrow={1}
                flexShrink={1}
                h={64}
                borderRadius="lg"
                p={4}
                cursor="pointer"
                onClick={onClickHandler}
                {...props}
            >
                {(() => {
                    if (type) {
                        switch (type.split('/')[0]) {
                            case 'Video':
                                const video = artifact.shards.filter((shard) => shard.kind === 'video')[0];

                                const square = (
                                    <Box
                                        flexGrow={1}
                                        borderRadius="md"
                                        bg={useColorModeValue('gray.100', 'gray.800')}
                                        borderColor={useColorModeValue('gray.300', 'gray.700')}
                                        borderWidth={1}
                                        borderStyle="solid"
                                        h="full"
                                    ></Box>
                                );

                                return (
                                    <VStack flexGrow={1} alignItems="stretch" w="full" spacing={4}>
                                        <HStack h={4} justifyItems="stretch" alignItems="stretch" spacing={4}>
                                            {square}
                                            {square}
                                            {square}
                                            {square}
                                            {square}
                                            {square}
                                        </HStack>
                                        <Box
                                            w="full"
                                            flexGrow={1}
                                            borderRadius="md"
                                            overflow="hidden"
                                            bg={useColorModeValue('gray.100', 'gray.800')}
                                            borderColor={useColorModeValue('gray.300', 'gray.700')}
                                            borderWidth={1}
                                            borderStyle="solid"
                                        >
                                            <Center h="full" color={useColorModeValue('gray.200', 'gray.900')}>
                                                <Icon as={IoVideocam} w={16} h={16} />
                                            </Center>
                                        </Box>
                                        <HStack h={4} justifyItems="stretch" alignItems="stretch" spacing={4}>
                                            {square}
                                            {square}
                                            {square}
                                            {square}
                                            {square}
                                            {square}
                                        </HStack>
                                    </VStack>
                                );
                            case 'Image':
                                const image = artifact.shards.filter((shard) => shard.kind === 'image')[0];

                                return (
                                    <Box
                                        flexGrow={1}
                                        w="full"
                                        borderRadius="md"
                                        overflow="hidden"
                                        bg={useColorModeValue('gray.100', 'gray.800')}
                                        borderColor={useColorModeValue('gray.300', 'gray.700')}
                                        borderWidth={1}
                                        borderStyle="solid"
                                        backgroundSize="cover"
                                        backgroundPosition="center"
                                        backgroundImage={`url(${api.shards.defaultStreamUrl(image.id)})`}
                                    ></Box>
                                );
                            case 'Blend':
                                return (
                                    <Box
                                        flexGrow={1}
                                        w="full"
                                        borderRadius="md"
                                        overflow="hidden"
                                        bg={useColorModeValue('gray.100', 'gray.800')}
                                        borderColor={useColorModeValue('gray.300', 'gray.700')}
                                        borderWidth={1}
                                        borderStyle="solid"
                                    >
                                        <Center h="full" color={useColorModeValue('gray.200', 'gray.900')}>
                                            <Icon as={SiBlender} w={16} h={16} />
                                        </Center>
                                    </Box>
                                );
                        }
                    }

                    return (
                        <Box
                            flexGrow={1}
                            w="full"
                            borderRadius="md"
                            overflow="hidden"
                            bg={useColorModeValue('gray.100', 'gray.800')}
                            borderColor={useColorModeValue('gray.300', 'gray.700')}
                            borderWidth={1}
                            borderStyle="solid"
                        >
                            <Center h="full" color={useColorModeValue('gray.200', 'gray.900')}>
                                <Icon as={IoWarning} w={16} h={16} />
                            </Center>
                        </Box>
                    );
                })()}
                <Box overflow="hidden" whiteSpace="nowrap" textOverflow="ellipsis" w="full" textAlign="center">
                    {artifact.getName()}
                </Box>
            </VStack>
            <ContentPopup artifact={artifact} isOpen={isOpen} onClose={onClose} />
        </>
    );
}
