import { Tag, TagLabel, TagLeftIcon } from '@chakra-ui/react';
import { t } from 'i18next';
import { AiOutlineEye, AiOutlineLock, AiOutlineQuestionCircle } from 'react-icons/ai';
import { BsShieldShaded } from 'react-icons/bs';
import { useColorScheme } from '../../hooks/useColorScheme';
import { components } from '../../schemas/api';

interface IVisibilityTagProps {
    visibility: unknown;
}

export function VisibilityTag(props: IVisibilityTagProps) {
    const { bgDarker } = useColorScheme();
    let Icon = AiOutlineQuestionCircle;

    switch (props.visibility) {
        case 'Private':
            Icon = AiOutlineLock;
            break;
        case 'Internal':
            Icon = BsShieldShaded;
            break;
        case 'Public':
            Icon = AiOutlineEye;
            break;
    }

    return (
        <Tag bg={bgDarker}>
            <TagLeftIcon boxSize="4" as={Icon} />
            <TagLabel>{t(`visibility.${props.visibility}`).toString()}</TagLabel>
        </Tag>
    );
}
