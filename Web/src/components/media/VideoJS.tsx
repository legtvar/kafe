import React from 'react';
import videojs from 'video.js';
import 'video.js/dist/video-js.css';

export interface IVideoJSProps {
    options: videojs.PlayerOptions;
    onReady?: (player: videojs.Player) => void;
}

export const VideoJS = (props: IVideoJSProps) => {
    const videoRef = React.createRef<HTMLVideoElement>();
    const [player, setPlayer] = React.useState<videojs.Player | null>(null);
    const { options, onReady } = props;

    React.useEffect(() => {
        // Make sure Video.js player is only initialized once
        if (!player) {
            const videoElement = videoRef.current;

            if (!videoElement) return;

            const player = videojs(videoElement, options, () => {
                videojs.log('Player is ready');
                onReady && onReady(player);
            });
            setPlayer(player);
        } else {
            options.sources && player.src(options.sources);
        }
    }, [options, videoRef, onReady, player]);

    React.useEffect(
        () => () => {
            if (player) player.dispose();
        },
        // eslint-disable-next-line react-hooks/exhaustive-deps
        [],
    );

    // Dispose the Video.js player when the functional component unmounts
    React.useEffect(() => {
        return () => {
            if (player) {
                player.dispose();
                setPlayer(null);
            }
        };
    }, [player]);

    return (
        <div data-vjs-player>
            <video ref={videoRef} className="video-js vjs-big-play-centered" />
        </div>
    );
};
