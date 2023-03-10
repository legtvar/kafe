import { Text } from '@chakra-ui/react';
import { t } from 'i18next';
import { Artifact } from '../../data/Artifact';
import { useApi } from '../../hooks/Caffeine';
import { Video } from './Player/Video';

interface IContentViewerProps {
    artifact: Artifact;
}

type ContentType = 'Video' | 'Image' | 'Unknown';

export function ContentViewer({ artifact }: IContentViewerProps) {
    const api = useApi();

    let type: ContentType = 'Unknown';

    // Determine the type of the content
    if (artifact.shards.some((shard) => shard.kind === 'Video')) {
        type = 'Video';
    } else if (artifact.shards.some((shard) => shard.kind === 'Image')) {
        type = 'Image';
    }

    if (type) {
        switch (type.split('/')[0]) {
            case 'Video':
                const video = artifact.shards.filter((shard) => shard.kind === 'Video')[0];
                // const subtitles = artifact.shards.filter((shard) => shard.kind === 'Subtitles')[0];

                const videoSources = video.variants.reduce(
                    (prev, curr) => ({
                        ...prev,
                        [curr]: api.shards.streamUrl(video.id, curr),
                    }),
                    {} as { [key: string]: string },
                );

                return <Video sources={videoSources} minW="100%" maxW="100%" h="60vmin" />;
            case 'Image':
                const image = artifact.shards.filter((shard) => shard.kind === 'Image')[0];
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
