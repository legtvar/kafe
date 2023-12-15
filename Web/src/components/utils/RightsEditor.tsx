import { Box, List, Spacer, Text } from '@chakra-ui/react';
import { t } from 'i18next';
import { AbstractType } from '../../data/AbstractType';
import { useAuth } from '../../hooks/Caffeine';
import { useColorScheme } from '../../hooks/useColorScheme';
import { Rights, RightsItem } from './RightsItem';

export interface IRightsEditorProps {
    item: AbstractType | null;
    readonly?: boolean;
    options: Array<Rights>
    explanation: Record<Rights, string>;
}

export function RightsEditor({ item, readonly, explanation }: IRightsEditorProps) {
    const { border } = useColorScheme();
    const { user } = useAuth();

    return (
        <>
            <Box mb={4} mx={-4} fontSize="smaller" color="gray.500">
                <List>
                    <li>
                        <Text as="span" fontWeight="bold">
                            {t('rights.write').toString()}
                        </Text>
                        <Text as="span"> - {explanation.write}</Text>
                    </li>
                    <li>
                        <Text as="span" fontWeight="bold">
                            {t('rights.read').toString()}
                        </Text>
                        <Text as="span"> - {explanation.read}</Text>
                    </li>
                    <li>
                        <Text as="span" fontWeight="bold">
                            {t('rights.inspect').toString()}
                        </Text>
                        <Text as="span"> - {explanation.inspect}</Text>
                    </li>
                    <li>
                        <Text as="span" fontWeight="bold">
                            {t('rights.append').toString()}
                        </Text>
                        <Text as="span"> - {explanation.append}</Text>
                    </li>
                </List>
                <Text mt={4}>{t('system.autosave').toString()}</Text>
            </Box>
            <Box>
                <RightsItem user={0} initialRights={[]} {...{ item, readonly }} />
                <Spacer borderBottomColor={border} borderBottomWidth={1} mx={-4} mt={1} />
                <RightsItem user={user!} initialRights={[Rights.READ]} {...{ item, readonly }} />
                <RightsItem user={user!} initialRights={[Rights.WRITE]} {...{ item, readonly }} />
                <RightsItem user={user!} initialRights={[Rights.APPEND]} {...{ item, readonly }} />
                <Spacer borderBottomColor={border} borderBottomWidth={1} mx={-4} mt={1} />
                <RightsItem user={null} initialRights={[]} {...{ item, readonly }} />
            </Box>
        </>
    );
}
