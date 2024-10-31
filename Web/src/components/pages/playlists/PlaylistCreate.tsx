import { Box, Heading, Stack } from '@chakra-ui/react';
import { t } from 'i18next';
import { useTitle } from '../../../utils/useTitle';
import { PlaylistBasicInfo } from './PlaylistBasicInfo';

interface IPlaylistCreateProps {}

export function PlaylistCreate(props: IPlaylistCreateProps) {
    useTitle(t('title.playlistCreate'));
    return (
        <Stack spacing={4} m={6} direction="column" pb={16}>
            <Box mb={2}>
                <Heading fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" mr="auto">
                    {t('playlistCreate.title').toString()}
                </Heading>
            </Box>
            <PlaylistBasicInfo />
        </Stack>
    );
}
