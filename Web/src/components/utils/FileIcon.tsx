import {
    AudioOutlined,
    CameraOutlined,
    FileExclamationOutlined,
    FileOutlined,
    FilePdfOutlined,
    FileTextOutlined,
    FileZipOutlined,
    VideoCameraOutlined,
} from '@ant-design/icons';

interface IFileIconProps {
    mimeType: string | null;
}

export function FileIcon(props: IFileIconProps) {
    if (!props.mimeType) {
        return <FileExclamationOutlined />;
    }

    switch (props.mimeType.split('/')[0]) {
        case 'video':
            return <VideoCameraOutlined />;
        case 'audio':
            return <AudioOutlined />;
        case 'image':
            return <CameraOutlined />;
        case 'text':
            return <FileTextOutlined />;
        case 'application':
            switch (props.mimeType.split('/')[1]) {
                case 'pdf':
                    return <FilePdfOutlined />;
                case 'zip':
                case 'vnd.rar':
                case 'x-7z-compressed':
                case 'gzip':
                    return <FileZipOutlined />;
            }
    }
    return <FileOutlined />;
}
