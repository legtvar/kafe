import { CalendarOutlined, GroupOutlined, LockOutlined } from '@ant-design/icons';
import { Empty, List, Skeleton } from 'antd';
import { t } from 'i18next';
import moment from 'moment';
import { Link } from 'react-router-dom';
import { Page } from '../../components/layout/Page';
import { AwaitAPI } from '../../components/utils/AwaitAPI';
import { IconText } from '../../components/utils/IconText';
import { Group } from '../../data/Group';
import { Project } from '../../data/Project';
import { truthy } from '../../utils/truthy';

interface IProjectsPageProps {}

export function ProjectsPage(props: IProjectsPageProps) {
    return (
        <Page title={t('projects.title')}>
            <AwaitAPI
                request={(caffeine) => Promise.all([caffeine.api.projects.getAll(), caffeine.api.groups.getAll()])}
                loader={
                    <List
                        itemLayout="vertical"
                        size="large"
                        dataSource={Array.from({ length: 10 }).map((_, i) => i)}
                        renderItem={(item) => (
                            <List.Item key={item}>
                                <Skeleton active />
                            </List.Item>
                        )}
                    />
                }
            >
                {([projects, groups]: [Project[], Group[]]) => (
                    <List
                        itemLayout="vertical"
                        size="large"
                        pagination={{}}
                        locale={{
                            emptyText: (
                                <Empty
                                    image={Empty.PRESENTED_IMAGE_SIMPLE}
                                    description={t('projects.empty').toString()}
                                />
                            ),
                        }}
                        dataSource={projects.sort((a, b) =>
                            !a.releaseDate
                                ? 1
                                : !b.releaseDate
                                ? -1
                                : b.releaseDate.getTime() - a.releaseDate.getTime(),
                        )}
                        renderItem={(item) => (
                            <Link to={`${item.id}`}>
                                <List.Item
                                    key={item.id}
                                    actions={truthy(
                                        <IconText
                                            icon={CalendarOutlined}
                                            text={
                                                item.releaseDate
                                                    ? moment(item.releaseDate).calendar()
                                                    : t('project.releaseDate.na').toString()
                                            }
                                        />,
                                        false && <IconText icon={LockOutlined} text={t('project.locked').toString()} />,
                                        // <IconText icon={FileOutlined} text={item.files.length} />,
                                        // <IconText icon={PlaySquareOutlined} text={item.playlists.length} />,
                                        <IconText
                                            icon={GroupOutlined}
                                            text={
                                                groups.filter((g) => g.id === item.projectGroupId)[0]?.getName() ||
                                                t('project.unknownGroup').toString()
                                            }
                                        />,
                                    )}
                                    // extra={
                                    //     <img
                                    //         width={272}
                                    //         height={100}
                                    //         style={{ objectFit: 'cover', objectPosition: 'center center' }}
                                    //         alt="Playlist thumbnail"
                                    //         src={`https://picsum.photos/id/${item.id}/500`}
                                    //     />
                                    // }
                                >
                                    <List.Item.Meta title={item.getName()} description={item.getDescription()} />
                                </List.Item>
                            </Link>
                        )}
                    />
                )}
            </AwaitAPI>
        </Page>
    );
}
