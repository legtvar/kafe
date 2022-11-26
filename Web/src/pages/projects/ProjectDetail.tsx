import { Avatar, Button, List, Typography } from 'antd';
import Paragraph from 'antd/lib/typography/Paragraph';
import { t } from 'i18next';
import React from 'react';
import { ProjectDataType } from '../../api/types';
import { Page } from '../../components/layout/Page';
import { Space } from '../../components/utils/Space';
import { VideoJS } from '../../components/VideoPlayer';
import { deepCopy } from '../../utils/deepCopy';
import { truthy } from '../../utils/truthy';

interface IProjectDetailProps {
    data: ProjectDataType;
}

export function ProjectDetail(props: IProjectDetailProps) {
    const [editing, setEditing] = React.useState(false);
    const [toSave, setToSave] = React.useState({});
    const { data } = props;

    const startEdit = () => {
        setEditing(true);
        setToSave(deepCopy(props.data));
    };

    const saveEdit = () => {
        setEditing(false);
        console.log(toSave);
    };

    const cancelEdit = () => {
        setEditing(false);
    };

    return (
        <Page
            title={data.title}
            headerProps={{
                extra: truthy(
                    !editing && (
                        <Button key="1" type="primary" onClick={startEdit}>
                            {t('common.edit').toString()}
                        </Button>
                    ),
                    editing && (
                        <Button key="1" onClick={cancelEdit}>
                            {t('common.cancel').toString()}
                        </Button>
                    ),
                    editing && (
                        <Button key="1" type="primary" onClick={saveEdit}>
                            {t('common.save').toString()}
                        </Button>
                    ),
                ),
            }}
        >
            <Typography.Title level={4}>{t('project.annotation').toString()}</Typography.Title>
            <Paragraph>
                <em>{data.genere}</em>
            </Paragraph>
            <Paragraph>{data.annotation}</Paragraph>

            <Space size="m" />

            <Typography.Title level={4}>{t('project.actors').toString()}</Typography.Title>
            <List
                dataSource={data.actors}
                renderItem={(item, i) => (
                    <List.Item>
                        <List.Item.Meta
                            avatar={<Avatar src={`https://joeschmoe.io/api/v1/${i}`} />}
                            title={item.name}
                            description={item.role}
                        />
                    </List.Item>
                )}
            />

            <Space size="l" />

            <Typography.Title level={4}>{t('project.crew').toString()}</Typography.Title>
            <List
                dataSource={data.crew}
                renderItem={(item, i) => (
                    <List.Item>
                        <List.Item.Meta
                            avatar={<Avatar src={`https://joeschmoe.io/api/v1/10${i}`} />}
                            title={item.name}
                            description={item.role}
                        />
                    </List.Item>
                )}
            />

            {/*
                - Název projektu
                - Žánr
                - Anotace
                - Filmový štáb
                - Herci
                - Film + Titulky
                - Grafika
                - Souhlasy
            */}

            <VideoJS
                options={{
                    autoplay: false,
                    controls: true,
                    responsive: true,
                    fill: true,
                    sources: [
                        {
                            src: '/kafe/jejky.mp4',
                            type: 'video/mp4',
                        },
                    ]
                }}
            />
        </Page>
    );
}
