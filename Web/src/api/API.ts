import { forTime } from 'waitasecond';
import { dummyProjectData } from './dummyData';

const delay = 200;

function getById(
    data: Array<{
        id: number;
    }>,
    id: number,
) {
    const found = data.filter((item) => item.id === id);
    if (found && found.length > 0) {
        return found[0];
    }
    return null;
}

async function simulateDelay(data: any) {
    await forTime(delay);
    return data;
}

export class API {
    public get projects() {
        return {
            async getAll() {
                return simulateDelay(dummyProjectData);
            },
            async getById(id: number) {
                return simulateDelay(getById(dummyProjectData, id));
            },
        };
    }
}
