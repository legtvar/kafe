import { Box, Center, Icon, Portal } from '@chakra-ui/react';
import { useEffect } from 'react';
import { IoClose } from 'react-icons/io5';
import { Artifact } from '../../data/Artifact';
import { ContentViewer } from './ContentViewer';

interface IContentPopupProps {
    artifact: Artifact;
    isOpen: boolean;
    onClose: () => void;
}

export function ContentPopup({ artifact, isOpen, onClose }: IContentPopupProps) {
    if (!isOpen) {
        return <></>;
    }

    // Handle escape key
    useEffect(() => {
        const handleKeyDown = (e: KeyboardEvent) => {
            if (e.key === 'Escape') {
                onClose();
            }
        };

        document.addEventListener('keydown', handleKeyDown);

        return () => {
            document.removeEventListener('keydown', handleKeyDown);
        };
    });

    return (
        <Portal>
            <Box
                position="fixed"
                inset={0}
                background="rgba(0,0,0,0.8)"
                backdropFilter="blur(10px)"
                zIndex="overlay"
            ></Box>
            <Center position="fixed" inset={0} zIndex="overlay">
                <ContentViewer artifact={artifact} width="calc(100% - 100px)" height="calc(100% - 100px)" />
            </Center>
            <Icon
                as={IoClose}
                onClick={onClose}
                position="fixed"
                top={4}
                right={4}
                fontSize={36}
                color="white"
                cursor="pointer"
                zIndex="overlay"
            />
        </Portal>
    );
}
