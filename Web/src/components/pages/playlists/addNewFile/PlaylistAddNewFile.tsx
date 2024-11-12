import { useCallback } from 'react';
import { API } from '../../../../api/API';
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
    const getGroups = useCallback((api: API) => api.groups.getAll(), []);

    return (
        <AwaitAPI request={useCallback((api) => api.projects.getAll(), [])} loader={<></>}>
            {(projects: Project[]) => (
                <AwaitAPI request={getGroups} loader={<></>}>
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
