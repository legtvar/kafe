export type ProjectDataType = {
    id: number;
    title: string;
    description: string;
    files: number;
    playlists: number;
    group: string;
    locked: boolean;
};

export type ProjectsDataType = ProjectDataType[];
