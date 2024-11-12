import { Box } from '@chakra-ui/react';
import axios from 'axios';
import { useEffect, useState } from 'react';
import subsrt from 'subsrt-ts';
import { Caption, ContentCaption } from 'subsrt-ts/dist/types/handler';
import { useApi } from '../../../hooks/Caffeine';

export interface ISubtitlesProps {
    path: string | null;
    currentSeconds: number;
}

export function Subtitles({ path, currentSeconds }: ISubtitlesProps) {
    const [subtitleData, setSubtitleData] = useState<Caption[] | null>(null);
    const api = useApi();

    console.log(currentSeconds);

    useEffect(() => {
        if (!path) return;

        setSubtitleData(null);

        (async () => {
            const response = await axios.get(path, {
                withCredentials: true,
            });
            const data = subsrt.parse(response.data);
            setSubtitleData(data);
            console.log(data);
        })();
    }, [path]);

    if (!subtitleData) return <></>;

    const currentSubtitles = subtitleData.filter(
        (caption) =>
            caption.type === 'caption' &&
            currentSeconds * 1000 >= caption.start &&
            currentSeconds * 1000 <= caption.end,
    ) as ContentCaption[];

    return (
        <div className="subtitles">
            {currentSubtitles.map((caption, index) => {
                const contentCaption = caption as ContentCaption;

                if (currentSeconds * 1000 >= contentCaption.start && currentSeconds * 1000 <= contentCaption.end) {
                    return (
                        <Box
                            key={index}
                            maxW="100%"
                            textAlign="center"
                            mt={8}
                            bg="rgba(0,0,0,0.6)"
                            px={4}
                            rounded="md"
                            fontSize="1.5cqw"
                        >
                            {contentCaption.text}
                        </Box>
                    );
                }

                return null;
            })}
        </div>
    );
}
