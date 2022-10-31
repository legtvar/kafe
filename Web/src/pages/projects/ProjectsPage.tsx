import { FileOutlined, GroupOutlined, LockOutlined, PlaySquareOutlined } from '@ant-design/icons';
import { List } from 'antd';
import { t } from 'i18next';
import { Link } from 'react-router-dom';
import { ProjectsDataType } from '../../api/types';
import { AwaitAPI } from '../../components/utils/AwaitAPI';
import { IconText } from '../../components/utils/IconText';
import { truthy } from '../../utils/truthy';

interface IProjectsPageProps {}

export function ProjectsPage(props: IProjectsPageProps) {
    return (
        <AwaitAPI request={(caffeine) => caffeine.api.projects.getAll()}>
            {(data: ProjectsDataType) => (
                <List
                    itemLayout="vertical"
                    size="large"
                    pagination={{
                        pageSize: 10,
                    }}
                    dataSource={data}
                    renderItem={(item) => (
                        <Link to={`${item.id}`}>
                            <List.Item
                                key={item.title}
                                actions={truthy(
                                    item.locked && (
                                        <IconText icon={LockOutlined} text={t('project.locked').toString()} />
                                    ),
                                    <IconText icon={FileOutlined} text={item.files} />,
                                    <IconText icon={PlaySquareOutlined} text={item.playlists} />,
                                    <IconText icon={GroupOutlined} text={item.group} />,
                                )}
                                extra={
                                    <img
                                        width={272}
                                        height={100}
                                        style={{ objectFit: 'cover', objectPosition: 'center center' }}
                                        alt="Playlist thumbnail"
                                        src={`https://picsum.photos/id/${item.id}/500`}
                                    />
                                }
                            >
                                <List.Item.Meta title={item.title} description={item.description} />
                            </List.Item>
                        </Link>
                    )}
                />
            )}
        </AwaitAPI>
    );
}
