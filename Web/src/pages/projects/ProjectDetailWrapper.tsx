import { Skeleton } from 'antd';
import { t } from 'i18next';
import { useParams } from 'react-router-dom';
import { ProjectDataType } from '../../api/types';
import { Error } from '../../components/layout/Error';
import { Page } from '../../components/layout/Page';
import { AwaitAPI } from '../../components/utils/AwaitAPI';
import { isNumeric } from '../../utils/isNumeric';
import { ProjectDetail } from './ProjectDetail';

interface IProjectDetailWrapperProps {}

export function ProjectDetailWrapper(props: IProjectDetailWrapperProps) {
    const { id } = useParams();

    if (!id || !isNumeric(id)) {
        return <Error error={t('error.invalidPath')} />;
    }
    const projectId = parseInt(id);

    return (
        <AwaitAPI
            request={(caffeine) => caffeine.api.projects.getById(projectId)}
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
            {(data: ProjectDataType | null) =>
                data ? <ProjectDetail data={data} /> : <Error error={t('error.projectDoesNotExist')} />
            }
        </AwaitAPI>
    );
}
