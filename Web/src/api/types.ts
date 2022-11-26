export type ProjectDataType = {
    id: number;
    title: string;
    annotation: string;
    genere: string;
    files: FileDataType[];
    playlists: any[];
    group: string;
    locked: boolean;
    actors: RoleDataType[];
    crew: RoleDataType[];
};

export type ProjectsDataType = ProjectDataType[];

export type FileDataType = {
    path: string;
    title: string;
    mime: string;
};

export type RoleDataType = {
    name: string;
    contact?: string;
    about?: string;
    role?: string;
};
