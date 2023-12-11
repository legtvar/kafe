import { Text } from '@chakra-ui/react';
import { t } from 'i18next';
import { ReactPlayerProps } from 'react-player';
import { Artifact } from '../../data/Artifact';
import { useApi } from '../../hooks/Caffeine';
import { Video } from './Player/Video';

interface IContentViewerProps {
    artifact: Artifact;
    autoplay?: boolean;
    videoProps?: ReactPlayerProps;
    onNext?: () => void;
    onPrevious?: () => void;
}

type ContentType = 'Video' | 'Image' | 'Unknown';

export function ContentViewer({ artifact, autoplay, videoProps, onPrevious, onNext }: IContentViewerProps) {
    const api = useApi();

    let type: ContentType = 'Unknown';

    // Determine the type of the content
    if (artifact.shards.some((shard) => shard.kind === 'video')) {
        type = 'Video';
    } else if (artifact.shards.some((shard) => shard.kind === 'image')) {
        type = 'Image';
    }

    if (type) {
        switch (type.split('/')[0]) {
            case 'video':
                const video = artifact.shards.filter((shard) => shard.kind === 'video')[0];
                const subtitles = artifact.shards.filter((shard) => shard.kind === 'subtitles');

                const videoSources = video.variants.reduce(
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
                        minW="100%"
                        maxW="100%"
                        h="60vmin"
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
                            width: '100%',
                            height: '60vmin',
                            objectFit: 'contain',
                            objectPosition: 'center center',
                        }}
                    />
                );
        }
    }

    return <Text color="gray.500">{t('content.unknownType').toString()}</Text>;
}
