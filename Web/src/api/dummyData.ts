import mime from 'mime';
import { ProjectsDataType } from './types';

export const dummyProjectData: ProjectsDataType = Array.from({ length: 15 }).map((_, i) => ({
    id: i,
    title: `Projekt bez názvu ${i}`,
    genere: 'Komedie',
    annotation:
        'Barevný zážitek trojice kamarádů naruší nedostatek bylin zvaných „parazit", povolávají a využívají přítele od Aničky – Pepika, který je nucen k zoufalým činům pouze proto, aby byli nakonec všichni spokojeni.',
    files: Array.from({ length: Math.random() * 6 }).map((_, j) => ({
        path: `/path/to/file/${j}`,
        title: `Soubor ${j}`,
        mime: mime.getType('mp4') || 'text/plain',
    })),
    playlists: Array.from({ length: Math.random() * 2 }).map((_, j) => j),
    group: `FFFI MU 20${i + 23 - 15}`,
    locked: i !== 14,
    actors: [
        { name: 'Viktor Marťan', role: 'Pepik' },
        { name: 'Gabriel Kulíšek', role: 'Vašek' },
        { name: 'Barbora Sedláčková', role: 'Anička' },
        { name: 'Radek Barták', role: 'Martin' },
    ],
    crew: [
        { name: 'Jaroslav Vůjtek', role: 'režie, scénář, kamera' },
        { name: 'Radek Barták', role: 'střih' },
        { name: 'Jakub Střelec', role: 'zvuk' },
        { name: 'Filip Neminarz', role: 'colorgrading' },
        { name: 'Daniela Ryšavá', role: 'skript' },
    ],
}));
