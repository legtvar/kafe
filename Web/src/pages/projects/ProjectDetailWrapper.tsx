import { Skeleton } from 'antd';
import { t } from 'i18next';
import { useParams } from 'react-router-dom';
import { Error } from '../../components/layout/Error';
import { Page } from '../../components/layout/Page';
import { AwaitAPI } from '../../components/utils/AwaitAPI';
import { Project } from '../../data/Project';
import { ProjectDetail } from './ProjectDetail';

interface IProjectDetailWrapperProps {}

export function ProjectDetailWrapper(props: IProjectDetailWrapperProps) {
    const { id } = useParams();

    if (!id) {
        return <Error error={t('error.invalidPath')} />;
    }

    return (
        <AwaitAPI
            request={(caffeine) => caffeine.api.projects.getById(id)}
            loader={
                <Page
                    headerProps={{
                        title: <Skeleton.Input active />,
                    }}
                >
                    <Skeleton active />
                </Page>
            }
        >
            {(data: Project | null) =>
                data ? <ProjectDetail data={data} /> : <Error error={t('error.projectDoesNotExist')} />
            }
        </AwaitAPI>
    );
}
