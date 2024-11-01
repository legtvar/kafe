import { Group } from '../../../../data/Group';
import { PlaylistEntry } from '../../../../data/Playlist';
import { Project } from '../../../../data/Project';
import { AwaitAPI } from '../../../utils/AwaitAPI';
import { PlaylistAddNewFileWrapper } from './PlaylistAddNewFileWrapper';

export interface IPlaylistAddNewFileProps {
    isOpen: boolean;
    onClose: (files: PlaylistEntry[]) => void;
}

export function PlaylistAddNewFile({ isOpen, onClose }: IPlaylistAddNewFileProps) {
    return (
        <AwaitAPI request={(api) => api.projects.getAll()} loader={<></>}>
            {(projects: Project[]) => (
                <AwaitAPI request={(api) => api.groups.getAll()} loader={<></>}>
                    {(groups: Group[]) => (
                        <PlaylistAddNewFileWrapper
                            isOpen={isOpen}
                            onClose={onClose}
                            groups={groups}
                            projects={projects}
                        />
                    )}
                </AwaitAPI>
            )}
        </AwaitAPI>
    );
}
