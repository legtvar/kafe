import { Avatar, Button, Card, Col, List, Row, Typography } from 'antd';
import { DataNode } from 'antd/lib/tree';
import DirectoryTree from 'antd/lib/tree/DirectoryTree';
import Paragraph from 'antd/lib/typography/Paragraph';
import { t } from 'i18next';
import React from 'react';
import { ContentViewer } from '../../components/ContentViewer';
import { Page } from '../../components/layout/Page';
import { FileIcon } from '../../components/utils/FileIcon';
import { Space } from '../../components/utils/Space';
import { Project } from '../../data/Project';
import { deepCopy } from '../../utils/deepCopy';
import { truthy } from '../../utils/truthy';

interface IProjectDetailProps {
    data: Project;
}

export function ProjectDetail(props: IProjectDetailProps) {
    const [editing, setEditing] = React.useState(false);
    const [toSave, setToSave] = React.useState({});
    const [selectedFile, setSelectedFile] = React.useState(0);
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

    const treeData: DataNode[] = [
        {
            title: data.getName(),
            key: 'root',
            children: data.getFiles().map((file, i) => ({
                title: file.getName(),
                key: i,
                isLeaf: true,
                icon: <FileIcon mimeType={file.getMime()} />,
            })),
        },
    ];

    return (
        <Page
            title={data.getName()}
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
            {/* <Paragraph>
                <em>{data.genere}</em>
            </Paragraph> */}
            <Paragraph>{data.getDescription()}</Paragraph>

            <Space size="m" />

            <Typography.Title level={4}>{t('project.files').toString()}</Typography.Title>
            <Paragraph>
                <Card>
                    <Row gutter={16} style={{ height: 400 }}>
                        <Col flex="300px">
                            <DirectoryTree
                                defaultExpandAll
                                onSelect={(keys) => keys[0] !== 'root' && setSelectedFile(keys[0] as number)}
                                selectedKeys={[selectedFile]}
                                treeData={treeData}
                            />
                        </Col>
                        <Col flex="auto" style={{ height: '100%' }}>
                            <ContentViewer file={data.getFiles()[selectedFile]} />
                        </Col>
                    </Row>
                </Card>
            </Paragraph>

            <Space size="m" />

            <Row gutter={16}>
                {/* <Col span={12}>
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
                </Col> */}
                <Col span={12}>
                    <Typography.Title level={4}>{t('project.crew').toString()}</Typography.Title>
                    <List
                        dataSource={data.authors}
                        renderItem={(item, i) => (
                            <List.Item>
                                <List.Item.Meta
                                    avatar={<Avatar src={`https://joeschmoe.io/api/v1/10${i}`} />}
                                    title={item}
                                    // description={item.role}
                                />
                            </List.Item>
                        )}
                    />
                </Col>
            </Row>

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
        </Page>
    );
}
