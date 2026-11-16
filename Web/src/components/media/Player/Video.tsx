import {
    Box,
    BoxProps,
    Center,
    DarkMode,
    Flex,
    Icon,
    IconButton,
    Menu,
    MenuButton,
    MenuItem,
    MenuList,
    Slider,
    SliderFilledTrack,
    SliderThumb,
    SliderTrack,
    Spacer,
    Spinner,
    Text,
    VStack,
} from '@chakra-ui/react';
import { t } from 'i18next';
import { createRef, useEffect, useState } from 'react';
import { AiOutlineCheck } from 'react-icons/ai';
import { BsDownload, BsGear } from 'react-icons/bs';
import {
    IoChatboxEllipsesOutline,
    IoEaselOutline,
    IoPauseOutline,
    IoPlayOutline,
    IoPlaySkipBackOutline,
    IoPlaySkipForwardOutline,
    IoScan,
    IoVolumeHighOutline,
    IoVolumeMuteOutline,
    IoWarning,
} from 'react-icons/io5';
import ReactPlayer from 'react-player';
import type { ReactPlayerProps } from 'react-player/types';
import screenfull from 'screenfull';
import { capitalize } from '../../../utils/capitalize';
import { Subtitles } from './Subtitles';

interface IVideoProps extends BoxProps {
    sources: { [key: string]: string };
    subtitles?: { [key: string]: string };
    autoplay?: boolean;
    videoProps?: ReactPlayerProps;
    onNext?: () => void;
    onPrevious?: () => void;
}

export function Video({ sources, subtitles, autoplay, videoProps, onNext, onPrevious, ...rest }: IVideoProps) {
    const [quality, setQuality] = useState(Object.keys(sources).includes('sd') ? 'sd' : Object.keys(sources)[0]);
    const [subtitleTrack, setSubtitleTrack] = useState<string | null>(null);
    const [playing, setPlaying] = useState(!!autoplay);
    const [progressState, setProgressState] = useState<{ playedSeconds: number; loaded: number } | null>(null);
    const [duration, setDuration] = useState(0);
    const [loaded, setLoaded] = useState(false);
    const [buffering, setBuffering] = useState(false);
    const [error, setError] = useState<any | null>(null);
    const [volume, setVolume] = useState(1);
    const [muted, setMuted] = useState(false);
    const [controlsShown, setControlsShown] = useState<NodeJS.Timeout | null>(null);
    const [lastClick, setLastClick] = useState(0);
    const [headless, setHeadless] = useState(false);
    const playerRef = createRef<HTMLVideoElement>();
    const wrapperRef = createRef<HTMLDivElement>();
    const keyboardHandlerRef = createRef<HTMLInputElement>();

    useEffect(() => {
        setLoaded(false);
        setBuffering(false);
        setError(null);
        setPlaying(true);
        if (playerRef.current) {
            playerRef.current.currentTime = progressState?.playedSeconds || 0;
        }

        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [subtitles, quality]);

    useEffect(() => {
        if (playerRef.current) {
            playerRef.current.currentTime = 0;
        }
        setQuality(Object.keys(sources).includes('sd') ? 'sd' : Object.keys(sources)[0]);
        setPlaying(!!autoplay);
        setProgressState(null);
        setDuration(0);
        setLoaded(false);
        setBuffering(false);
        setError(null);
        setSubtitleTrack(null);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [sources, autoplay]);

    const updateMouse = () => {
        if (controlsShown) clearTimeout(controlsShown);
        setControlsShown(setTimeout(() => setControlsShown(null), 2000));
    };

    return (
        <DarkMode>
            <Box
                ref={wrapperRef}
                width={'100%'}
                height={'100%'}
                position="relative"
                background="black"
                outline={0}
                zIndex="video-base"
                onClick={() => {
                    wrapperRef.current?.focus();
                }}
                color="white"
                onKeyDown={(e) => {
                    if (playerRef.current) {
                        if (e.key === ' ') {
                            setPlaying(!playing);
                            updateMouse();
                        } else if (e.key === 'ArrowUp') {
                            setVolume(Math.min(1, volume + 0.05));
                            updateMouse();
                        } else if (e.key === 'ArrowDown') {
                            setVolume(Math.max(0, volume - 0.05));
                            updateMouse();
                        } else if (e.key === 'm') {
                            setMuted(!muted);
                            updateMouse();
                        } else if (e.key === 'f') {
                            screenfull.toggle(wrapperRef.current!);
                            updateMouse();
                        } else if (e.key === 'ArrowRight') {
                            playerRef.current!.currentTime = (progressState?.playedSeconds || 0) + 5;
                            updateMouse();
                        } else if (e.key === 'ArrowLeft') {
                            playerRef.current!.currentTime = (progressState?.playedSeconds || 0) - 5;
                            updateMouse();
                        }
                    }
                    e.preventDefault();
                    e.stopPropagation();
                }}
                tabIndex={0}
                {...rest}
            >
                <ReactPlayer
                    {...videoProps}
                    ref={playerRef}
                    width={'100%'}
                    height={'100%'}
                    playing={playing}
                    volume={volume}
                    muted={muted}
                    src={sources[quality]}
                    crossOrigin="use-credentials"
                    onError={(event) => {
                        setPlaying(false);
                        setError(event.currentTarget.error?.message || t('player.loadingError'));
                    }}
                    onProgress={() => {
                        if (playerRef.current) {
                            const player = playerRef.current;
                            const loaded = player.buffered.length > 0 && player.duration > 0
                                ? player.buffered.end(player.buffered.length - 1) / player.duration
                                : 0;

                            setProgressState({ playedSeconds: player.currentTime, loaded });
                        }
                    }}
                    onTimeUpdate={() => {
                        if (playerRef.current) {
                            const player = playerRef.current;

                            setProgressState((previous) => ({
                                playedSeconds: player.currentTime,
                                loaded: previous?.loaded || 0,
                            }));
                            setDuration(player.duration || 0);
                            setBuffering(false);
                        }
                    }}
                    onDurationChange={(event) => setDuration(event.currentTarget.duration || 0)}
                    onWaiting={() => setBuffering(true)}
                    onPlaying={() => setBuffering(false)}
                    onPlay={() => {
                        setPlaying(true);
                        setLoaded(true);
                        setError(null);
                    }}
                    onPause={() => setPlaying(false)}
                    onReady={() => setLoaded(true)}
                >
                    {(subtitles ? Object.entries(subtitles) : []).map(([id, url]) => (
                        <track key={id} kind="subtitles" src={url} srcLang="en" label={id} />
                    ))}
                </ReactPlayer>
                <Flex
                    position="absolute"
                    left="25%"
                    right="25%"
                    top="16"
                    bottom="16"
                    flexDir="column"
                    color="white"
                    fontSize="2em"
                    alignItems="center"
                    zIndex="video-subtitles"
                >
                    <Spacer />
                    {subtitles && subtitleTrack && progressState && (
                        <Subtitles path={subtitles[subtitleTrack]} currentSeconds={progressState.playedSeconds} />
                    )}
                </Flex>
                {!headless && (
                    <Center
                        position="absolute"
                        top={0}
                        left={0}
                        bottom={0}
                        right={0}
                        background={error ? 'rgba(0,0,0,0.8)' : undefined}
                        zIndex="video-overlay"
                    >
                        {}
                        {error ? (
                            <VStack>
                                <Icon color={'yellow.500'} as={IoWarning} fontSize="5em" />
                                <Text>{error.toString()}</Text>
                            </VStack>
                        ) : (
                            (buffering || !loaded) && <Spinner size={'xl'} />
                        )}
                    </Center>
                )}
                {true && (
                    <Box
                        position="absolute"
                        inset={0}
                        opacity={controlsShown || !playing ? 1 : 0}
                        cursor={(controlsShown || !playing) && !headless ? 'default' : 'none'}
                        onMouseMove={() => updateMouse()}
                        onTouchMove={() => updateMouse()}
                        transition="opacity 0.2s ease-in-out"
                        zIndex="video-controls"
                    >
                        <Box
                            position="absolute"
                            inset={0}
                            onClick={() => {
                                if (lastClick + 500 > Date.now()) {
                                    screenfull.toggle(wrapperRef.current!);
                                }
                                setPlaying(!playing);
                                setLastClick(Date.now());
                                keyboardHandlerRef.current?.focus();
                            }}
                        >
                            {/* Click detector over player */}
                        </Box>
                        {!headless && (
                            <Flex
                                px={4}
                                position="absolute"
                                bottom={0}
                                left={0}
                                right={0}
                                bg="rgba(0,0,0,0.8)"
                                cursor="default"
                                alignItems={{
                                    base: 'stretch',
                                    lg: 'center',
                                }}
                                direction={{
                                    base: 'column',
                                    lg: 'row',
                                }}
                                flexWrap={{
                                    base: 'nowrap',
                                    lg: 'wrap',
                                }}
                                zIndex="video-controls"
                            >
                                <Flex
                                    direction="row"
                                    alignItems="center"
                                    flexGrow={1}
                                    h={{
                                        base: 10,
                                        lg: 14,
                                    }}
                                >
                                    <IconButton
                                        variant="ghost"
                                        aria-label="Play"
                                        onClick={() => {
                                            setPlaying(!playing);
                                            updateMouse();
                                        }}
                                        tabIndex={-1}
                                        icon={playing ? <IoPauseOutline /> : <IoPlayOutline />}
                                    />
                                    {onPrevious && (
                                        <IconButton
                                            variant="ghost"
                                            aria-label="Previous"
                                            onClick={() => onPrevious()}
                                            tabIndex={-1}
                                            icon={<IoPlaySkipBackOutline />}
                                        />
                                    )}
                                    {onNext && (
                                        <IconButton
                                            variant="ghost"
                                            aria-label="Next"
                                            onClick={() => onNext()}
                                            tabIndex={-1}
                                            icon={<IoPlaySkipForwardOutline />}
                                        />
                                    )}
                                    <Text
                                        whiteSpace="nowrap"
                                        ml={4}
                                        display={{
                                            base: 'none',
                                            lg: 'block',
                                        }}
                                    >
                                        {progressState
                                            ? `${formatTime(progressState.playedSeconds)} / ${formatTime(duration)}`
                                            : '00:00 / 00:00'}
                                    </Text>
                                    <Slider
                                        ml={4}
                                        aria-label="slider-ex-1"
                                        value={progressState?.playedSeconds}
                                        max={duration}
                                        step={0.01}
                                        tabIndex={-1}
                                        onChange={(value) => {
                                            if (playerRef.current) {
                                                playerRef.current.currentTime = value;
                                            }
                                            updateMouse();
                                        }}
                                        focusThumbOnChange={false}
                                    >
                                        <SliderTrack>
                                            <Box
                                                position="absolute"
                                                top={0}
                                                bottom={0}
                                                left={0}
                                                w={`${(progressState?.loaded || 0) * 100}%`}
                                                bg="rgba(255,255,255,0.4)"
                                            ></Box>
                                            <SliderFilledTrack />
                                        </SliderTrack>
                                        <SliderThumb />
                                    </Slider>
                                </Flex>
                                <Flex
                                    direction="row"
                                    alignItems="center"
                                    h={{
                                        base: 10,
                                        lg: 14,
                                    }}
                                    ml={{
                                        base: -6,
                                        lg: 0,
                                    }}
                                >
                                    <IconButton
                                        ml={4}
                                        variant="ghost"
                                        aria-label="Mute"
                                        onClick={() => setMuted(!muted)}
                                        tabIndex={-1}
                                        icon={muted ? <IoVolumeMuteOutline /> : <IoVolumeHighOutline />}
                                    />
                                    <Slider
                                        ml={{ base: 2, lg: 1 }}
                                        value={volume}
                                        max={1}
                                        w={{
                                            base: 16,
                                            xl: 32,
                                        }}
                                        step={0.001}
                                        tabIndex={-1}
                                        onChange={(value) => {
                                            setVolume(value as number);
                                            updateMouse();
                                        }}
                                        focusThumbOnChange={false}
                                    >
                                        <SliderTrack>
                                            <SliderFilledTrack />
                                        </SliderTrack>
                                        <SliderThumb />
                                    </Slider>
                                    {subtitles && (
                                        <Menu strategy="absolute">
                                            <MenuButton
                                                variant="ghost"
                                                as={IconButton}
                                                aria-label="Subtitles"
                                                icon={<IoChatboxEllipsesOutline />}
                                                tabIndex={-1}
                                                ml={{ base: 6, lg: 3 }}
                                            />
                                            <MenuList>
                                                <MenuItem
                                                    icon={null === subtitleTrack ? <AiOutlineCheck /> : undefined}
                                                    onClick={() => setSubtitleTrack(null)}
                                                    tabIndex={-1}
                                                >
                                                    {t('subtitles.none').toString()}
                                                </MenuItem>
                                                {Object.keys(subtitles).map((source, key) => (
                                                    <MenuItem
                                                        key={key}
                                                        value={source}
                                                        icon={source === subtitleTrack ? <AiOutlineCheck /> : undefined}
                                                        onClick={() => setSubtitleTrack(source)}
                                                        tabIndex={-1}
                                                    >
                                                        {capitalize(source)}
                                                    </MenuItem>
                                                ))}
                                            </MenuList>
                                        </Menu>
                                    )}
                                    <Menu strategy="absolute">
                                        <MenuButton
                                            variant="ghost"
                                            as={IconButton}
                                            aria-label="Quality"
                                            icon={<BsGear />}
                                            tabIndex={-1}
                                            ml={{ base: 6, lg: 3 }}
                                        />
                                        <MenuList>
                                            {Object.keys(sources).map((source, key) => (
                                                <MenuItem
                                                    key={key}
                                                    value={source}
                                                    icon={source === quality ? <AiOutlineCheck /> : undefined}
                                                    onClick={() => setQuality(source)}
                                                    tabIndex={-1}
                                                >
                                                    {capitalize(source)}
                                                </MenuItem>
                                            ))}
                                        </MenuList>
                                    </Menu>
                                    <Menu strategy="absolute">
                                        <MenuButton
                                            variant="ghost"
                                            as={IconButton}
                                            aria-label="Download"
                                            icon={<BsDownload />}
                                            ml={{ base: 6, lg: 3 }}
                                            tabIndex={-1}
                                            display={{
                                                base: 'none',
                                                xl: 'flex',
                                            }}
                                        />
                                        <MenuList>
                                            {Object.keys(sources).map((source, key) => (
                                                <MenuItem
                                                    key={key}
                                                    as={'a'}
                                                    value={source}
                                                    href={`${sources[source]}`}
                                                    target="_blank"
                                                    tabIndex={-1}
                                                >
                                                    {capitalize(source)}
                                                </MenuItem>
                                            ))}
                                        </MenuList>
                                    </Menu>
                                    {/* <Link
                                    tabIndex={-1}
                                    to={`/play/${sources['original'].split('/').reverse()[1]}`}
                                    target="_blank"
                                >
                                    <IconButton
                                        ml={2}
                                        display={{
                                            base: 'none',
                                            lg: 'flex',
                                        }}
                                        variant="ghost"
                                        aria-label="Share"
                                        icon={<IoLink />}
                                        tabIndex={-1}
                                    />
                                </Link> */}
                                    <IconButton
                                        ml={{ base: 6, lg: 2 }}
                                        variant="ghost"
                                        aria-label="Headless fullscreen"
                                        tabIndex={-1}
                                        onClick={() => {
                                            if (headless) {
                                                setHeadless(false);
                                                screenfull.exit();
                                            } else {
                                                setHeadless(true);
                                                screenfull.request(wrapperRef.current!);
                                                screenfull.on('change', () => {
                                                    if (!screenfull.isFullscreen) {
                                                        setHeadless(false);
                                                    }
                                                });
                                            }
                                        }}
                                        icon={<IoEaselOutline />}
                                    />
                                    <IconButton
                                        ml={{ base: 2, lg: 1 }}
                                        variant="ghost"
                                        aria-label="Fullscreen"
                                        onClick={() => screenfull.toggle(wrapperRef.current!)}
                                        tabIndex={-1}
                                        icon={<IoScan />}
                                    />
                                </Flex>
                            </Flex>
                        )}
                    </Box>
                )}
            </Box>
        </DarkMode>
    );
}

function formatTime(seconds: number) {
    return `${Math.floor(seconds / 60)
        .toString()
        .padStart(2, '0')}:${Math.floor(seconds % 60)
        .toString()
        .padStart(2, '0')}`;
}
