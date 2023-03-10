import { Project } from '../../../data/Project';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { ProjectListComponent } from './ProjectListComponent';

interface IProjectsProps {}

export function Projects(props: IProjectsProps) {
    return (
        <OutletOrChildren>
            <AwaitAPI request={(api) => api.projects.getAll()}>
                {(data: Project[]) => <ProjectListComponent projects={data} />}
            </AwaitAPI>
        </OutletOrChildren>
    );
}
