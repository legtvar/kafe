export const dummyProjectData = Array.from({ length: 15 }).map((_, i) => ({
    id: i,
    title: `Projekt bez názvu ${i}`,
    description: 'Tady je volitelný popis projektu, který může být i delší',
    files: Math.floor(Math.random() * 10),
    playlists: Math.floor(Math.random() * 2),
    group: `FFFI MU 20${i + 23 - 15}`,
    locked: i !== 14,
}));
