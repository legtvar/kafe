import { useParams } from 'react-router-dom';
import { Video } from '../../components/media/Player/Video';
import { useApi } from '../../hooks/Caffeine';
import { Status } from '../utils/Status';

interface IPlayerProps {}

export function Player(props: IPlayerProps) {
    const { slug } = useParams();
    const api = useApi();

    if (!slug) {
        return <Status statusCode={404} embeded />;
    }

    return (
        <Video
            sources={{ original: api.shards.streamUrl(slug, 'original') }}
            minW="100%"
            maxW="100%"
            h="100vh"
            bg="black"
        />
    );
}
