import { Space } from 'antd';
import React from 'react';

export const IconText = ({ icon, text }: { icon: React.FC; text: string | number }) => (
    <Space>
        {React.createElement(icon)}
        {text}
    </Space>
);
