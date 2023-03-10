import { AiOutlineFile } from 'react-icons/ai';
import { BsCameraReels, BsCardImage, BsChatLeftText } from 'react-icons/bs';
import { components } from '../../schemas/api';

interface IFileIconProps {
    kind: components['schemas']['ShardKind'];
}

export function FileIcon(props: IFileIconProps) {
    switch (props.kind) {
        case 'Video':
            return <BsCameraReels />;

        case 'Image':
            return <BsCardImage />;

        case 'Subtitles':
            return <BsChatLeftText />;

        case 'Unknown':
            return <AiOutlineFile />;
    }
}
