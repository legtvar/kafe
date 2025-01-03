import { t } from 'i18next';
import { useCallback } from 'react';
import { Project } from '../../../data/Project';
import { useOrganizations } from '../../../hooks/Caffeine';
import { useTitle } from '../../../utils/useTitle';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { ProjectListComponent } from './ProjectListComponent';

interface IProjectsProps {}

export function Projects(props: IProjectsProps) {
    useTitle(t('title.projects'));
    const { currentOrganization } = useOrganizations();

    return (
        <OutletOrChildren>
            <AwaitAPI
                request={useCallback((api) => api.projects.getAll(currentOrganization?.id), [currentOrganization])}
            >
                {(data: Project[]) => <ProjectListComponent projects={data} />}
            </AwaitAPI>
        </OutletOrChildren>
    );
}
