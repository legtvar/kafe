import { Button, Center, Text, VStack } from '@chakra-ui/react';
import { t } from 'i18next';
import { AiOutlineDownload } from 'react-icons/ai';
import { SiBlender } from "react-icons/si";
import { ReactPlayerProps } from 'react-player';
import { Artifact } from '../../data/Artifact';
import { Shard } from '../../data/Shard';
import { useApi } from '../../hooks/Caffeine';
import { Video } from './Player/Video';

interface IContentViewerProps {
    artifact: Artifact;
    autoplay?: boolean;
    videoProps?: ReactPlayerProps;
    onNext?: () => void;
    onPrevious?: () => void;
    height?: string;
    width?: string;
}

type ContentType = 'Video' | 'Image' | 'Unknown' | 'Blend';

export function ContentViewer({
    artifact,
    autoplay,
    videoProps,
    onPrevious,
    onNext,
    width,
    height,
}: IContentViewerProps) {
    const api = useApi();

    let type: ContentType = 'Unknown';

    // Determine the type of the content
    if (artifact.shards.some((shard) => shard.kind === 'video')) {
        type = 'Video';
    } else if (artifact.shards.some((shard) => shard.kind === 'image')) {
        type = 'Image';
    } else if (artifact.shards.some((shard) => shard.kind === 'blend')) {
        type = 'Blend';
    }

    if (type) {
        switch (type.split('/')[0]) {
            case 'Video':
                let video = artifact.shards.filter((shard) => shard.kind === 'video')[0];
                let subtitles = artifact.shards.filter((shard) => shard.kind === 'subtitles');

                video = new Shard(video);
                subtitles = subtitles.map((sub) => new Shard(sub));

                const videoSources = video.getVariantIds().reduce(
                    (prev, curr) => ({
                        ...prev,
                        [curr]: api.shards.streamUrl(video.id, curr),
                    }),
                    {} as { [key: string]: string },
                );

                const subtitleSources = subtitles.reduce(
                    (prev, curr) => ({
                        ...prev,
                        [curr.id]: api.shards.defaultStreamUrl(curr.id),
                    }),
                    {} as { [key: string]: string },
                );

                return (
                    <Video
                        videoProps={videoProps}
                        sources={videoSources}
                        subtitles={subtitleSources}
                        minW={width || '100%'}
                        maxW={width || '100%'}
                        h={height || '60vmin'}
                        autoplay={autoplay}
                        onPrevious={onPrevious}
                        onNext={onNext}
                    />
                );
            case 'Image':
                const image = artifact.shards.filter((shard) => shard.kind === 'image')[0];
                return (
                    <img
                        src={api.shards.defaultStreamUrl(image.id)}
                        alt={artifact.getName()}
                        style={{
                            width: width || '100%',
                            height: height || '60vmin',
                            objectFit: 'contain',
                            objectPosition: 'center center',
                        }}
                    />
                );

            case 'Blend':
                return (
                    <VStack spacing={6}>
                        <Center h="full" color={'gray.200'}>
                            <SiBlender size={64} />
                        </Center>
                        {artifact.shards.map((shard) => (
                            <Button
                                key={shard.id}
                                as={'a'}
                                href={api.shards.defaultStreamUrl(shard.id)}
                                leftIcon={<AiOutlineDownload />}
                            >
                                {t('generic.download')} {t('generic.file')} {shard.id}
                            </Button>
                        ))}
                        {artifact.shards.length === 0 && (
                            <Text color="gray.500" fontStyle="italic">
                                {t('artifact.noContent').toString()}
                            </Text>
                        )}
                    </VStack>
                );
        }
    }

    return (
        <VStack spacing={6}>
            <Text color="gray.500">{t('content.unknownType').toString()}</Text>
            {artifact.shards.map((shard) => (
                <Button
                    key={shard.id}
                    as={'a'}
                    href={api.shards.defaultStreamUrl(shard.id)}
                    leftIcon={<AiOutlineDownload />}
                >
                    {t('generic.download')} {t('generic.file')} {shard.id}
                </Button>
            ))}
            {artifact.shards.length === 0 && (
                <Text color="gray.500" fontStyle="italic">
                    {t('artifact.noContent').toString()}
                </Text>
            )}
        </VStack>
    );
}
