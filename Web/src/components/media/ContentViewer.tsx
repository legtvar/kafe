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
                const subtitles = artifact.shards.filter((shard) => shard.kind === 'Subtitles')[0];

                const videoSources = video.variants.reduce(
                    (prev, curr) => ({
                        ...prev,
                        [curr]: api.shards.streamUrl(video.id, curr),
                    }),
                    {} as { [key: string]: string },
                );

                console.log(videoSources);

                return <Video sources={videoSources} minW="100%" maxW="100%" h="60vmin" />;
            case 'Image':
                return (
                    <img
                        src={'https://video-react.js.org/assets/poster.png'}
                        alt={artifact.getName()}
                        style={{
                            width: '100%',
                            height: '100%',
                            objectFit: 'contain',
                            objectPosition: 'center center',
                        }}
                    />
                );
        }
    }

    return <>{t('content.unknownType').toString()}</>;
}
