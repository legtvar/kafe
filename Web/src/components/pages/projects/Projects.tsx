import { t } from 'i18next';
import { Project } from '../../../data/Project';
import { useTitle } from '../../../utils/useTitle';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { ProjectListComponent } from './ProjectListComponent';

interface IProjectsProps {}

export function Projects(props: IProjectsProps) {
    useTitle(t("title.projects"));
    return (
        <OutletOrChildren>
            <AwaitAPI request={(api) => api.projects.getAll()}>
                {(data: Project[]) => <ProjectListComponent projects={data} />}
            </AwaitAPI>
        </OutletOrChildren>
    );
}
