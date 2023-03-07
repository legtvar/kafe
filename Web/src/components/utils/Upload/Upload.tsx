import { Box, Center, Flex, keyframes, Text, useColorModeValue, useToken } from '@chakra-ui/react';
import { motion } from 'framer-motion';
import { useState } from 'react';
import Dropzone from 'react-dropzone';
import { Trans } from 'react-i18next';
import { BsCloudArrowUp, BsCloudArrowUpFill, BsCloudCheckFill } from 'react-icons/bs';
import { useApi } from '../../../hooks/Caffeine';
import { components } from '../../../schemas/api';

interface IUploadProps {
    title: string;
    projectId: string;
    shardKind: components['schemas']['ShardKind'];
    getArtifactId: () => Promise<string>;
}

export function Upload(props: IUploadProps) {
    const [status, setStatus] = useState<'ready' | 'uploading' | 'uploaded' | 'error'>('ready');
    const [progress, setProgress] = useState<number>(0);
    const api = useApi();

    async function upload<T extends File>(files: T[]) {
        if (status !== 'ready') return;
        if (files.length < 1) return;

        setStatus('uploading');

        const artifactId = await props.getArtifactId();

        const response = await api.shards.create(artifactId, files[0], 'Video', (event) =>
            setProgress(event.progress || 0),
        );

        if (response.status === 200) {
            setStatus('uploaded');
        } else {
            setStatus('error');
            // TODO: Display error
        }
    }

    const animationKeyframes = keyframes`
        0% { color: ${useToken('colors', useColorModeValue('gray.200', 'gray.800'))} }
        50% { color: ${useToken('colors', useColorModeValue('gray.500', 'gray.500'))} }
        100% { color: ${useToken('colors', useColorModeValue('gray.200', 'gray.800'))} }
    `;

    const animation = `${animationKeyframes} 2s ease-in-out infinite`;

    if (status === 'ready')
        return (
            <Dropzone onDrop={upload} maxFiles={1}>
                {({ getRootProps, getInputProps, isDragActive, isFileDialogActive }) => (
                    <Center
                        py={8}
                        {...getRootProps()}
                        textColor={isDragActive || isFileDialogActive ? undefined : 'gray.500'}
                        cursor="pointer"
                    >
                        <Flex direction="column" alignItems="center">
                            <Box fontSize={96}>
                                {isDragActive || isFileDialogActive ? <BsCloudArrowUpFill /> : <BsCloudArrowUp />}
                            </Box>
                            <Text align="center">
                                <Trans i18nKey="upload.ready" values={{ title: props.title }}>
                                    Sem přetáhněte
                                    <Text display="inline" fontWeight="bold">
                                        "title"
                                    </Text>
                                    <br />
                                    nebo klikněte pro výběr z počítače
                                    <input {...getInputProps()} />
                                </Trans>
                            </Text>
                        </Flex>
                    </Center>
                )}
            </Dropzone>
        );

    if (status === 'uploading')
        return (
            <Center py={16} h="300px">
                <Flex direction="column" alignItems="center">
                    <Box as={motion.div} fontSize={96} animation={animation}>
                        <BsCloudArrowUpFill />
                    </Box>
                    <Text align="center">
                        <Trans i18nKey="upload.uploading" values={{ progress: (progress * 100).toFixed(0) }}>
                            <Text display="inline" fontWeight="bold">
                                Video se nahrává
                            </Text>
                            <br />
                            Nahráno "progress"%
                        </Trans>
                    </Text>
                </Flex>
            </Center>
        );

    return (
        <Center py={16} h="300px">
            <Flex direction="column" alignItems="center">
                <Box fontSize={96} color="green.500">
                    <BsCloudCheckFill />
                </Box>
                <Text align="center">
                    <Trans i18nKey="upload.uploaded">
                        <Text display="inline" fontWeight="bold">
                            Úspěšně nahráno
                        </Text>
                        <br />
                        Formulář už můžete odeslat
                    </Trans>
                </Text>
            </Flex>
        </Center>
    );
}
