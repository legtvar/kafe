import { AiOutlineFile } from 'react-icons/ai';
import { BsCameraReels, BsCardImage, BsChatLeftText } from 'react-icons/bs';
import { components } from '../../schemas/api';

interface IFileIconProps {
    kind: components['schemas']['ShardKind'];
}

export function FileIcon(props: IFileIconProps) {
    switch (props.kind) {
        case 'video':
            return <BsCameraReels />;

        case 'image':
            return <BsCardImage />;

        case 'subtitles':
            return <BsChatLeftText />;

        case 'unknown':
            return <AiOutlineFile />;
    }
}
