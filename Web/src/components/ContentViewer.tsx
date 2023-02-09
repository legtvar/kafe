import axios from 'axios';
import { t } from 'i18next';
import { FileEntry } from '../data/FileEntry';
import { Await } from './utils/Await';
import { VideoJS } from './VideoJS';

interface IContentViewerProps {
    file: FileEntry;
}

export function ContentViewer(props: IContentViewerProps) {
    if (props.file) {
        const type = props.file.getMime();

        if (type) {
            switch (type.split('/')[0]) {
                case 'video':
                    return (
                        <VideoJS
                            options={{
                                autoplay: false,
                                controls: true,
                                responsive: true,
                                fill: true,
                                sources: [
                                    {
                                        src: props.file.path,
                                        type: props.file.getMime()!,
                                    },
                                ],
                            }}
                        />
                    );
                case 'image':
                    return (
                        <img
                            src={props.file.path}
                            alt={props.file.getName()}
                            style={{
                                width: '100%',
                                height: '100%',
                                objectFit: 'contain',
                                objectPosition: 'center center',
                            }}
                        />
                    );
                case 'text':
                    return <Await for={axios.get(props.file.path)}>{(response) => response.data}</Await>;
            }
        }

        return t('content.unknownType').toString();
    }
    return t('content.noFileSelected').toString();
}
