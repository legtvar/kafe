import Paragraph from 'antd/lib/typography/Paragraph';
import Title from 'antd/lib/typography/Title';
import { t } from 'i18next';
import { useParams } from 'react-router-dom';
import { ProjectDataType } from '../../api/types';
import { Error } from '../../components/layout/Error';
import { AwaitAPI } from '../../components/utils/AwaitAPI';
import { VideoJS } from '../../components/VideoPlayer';
import { isNumeric } from '../../utils/isNumeric';

interface IProjectDetailProps {}

export function ProjectDetail(props: IProjectDetailProps) {
    const { id } = useParams();

    if (!id || !isNumeric(id)) {
        return <Error error={t('error.invalidPath')} />;
    }
    const projectId = parseInt(id);

    return (
        <AwaitAPI request={(caffeine) => caffeine.api.projects.getById(projectId)}>
            {(data: ProjectDataType | null) =>
                data ? (
                    <>
                        <Title level={3}>{data.title}</Title>
                        <Paragraph>{data.description}</Paragraph>

                        <VideoJS
                            options={{
                                autoplay: false,
                                controls: true,
                                responsive: true,
                                fluid: false,
                                sources: [
                                    {
                                        src: '/kafe/jejky.mp4',
                                        type: 'video/mp4',
                                    },
                                ],
                                width: 500,
                            }}
                        />
                    </>
                ) : (
                    <Error error={t('error.projectDoesNotExist')} />
                )
            }
        </AwaitAPI>
    );
}
