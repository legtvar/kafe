import { Box, HStack, Text, VStack } from '@chakra-ui/react';
import { t } from 'i18next';
import { BsCheckCircleFill, BsFillExclamationCircleFill } from 'react-icons/bs';
import { Project } from '../../../data/Project';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { getPrefered } from '../../../utils/preferedLanguage';
import { DateTime } from 'luxon';

interface IReviewListProps {
    project: Project;
}

export function ReviewList({ project }: IReviewListProps) {
    const { border, bg } = useColorScheme();

    return (
        <VStack w="100%" alignItems="stretch" spacing={4}>
            {project.reviews
                .sort((a, b) => new Date(b.addedOn).getTime() - new Date(a.addedOn).getTime())
                .map((review, i) => (
                    <HStack
                        borderColor={border}
                        bg={bg}
                        borderWidth={1}
                        borderRadius="md"
                        px={6}
                        py={4}
                        spacing={4}
                        alignItems="start"
                        key={i}
                    >
                        <Box fontSize="2em" mt={2}>
                            {review.kind === 'accepted' ? (
                                <Text color="green.500">
                                    <BsCheckCircleFill />
                                </Text>
                            ) : (
                                <Text color="red.500">
                                    <BsFillExclamationCircleFill />
                                </Text>
                            )}
                        </Box>
                        <Box>
                            <Text fontWeight="bold">{t(`project.admin.role.${review.reviewerRole}`).toString()}</Text>
                            {review.addedOn && (
                                <Text color="gray.500" pb={4}>
                                    {DateTime.fromISO(review.addedOn).toLocaleString()}
                                </Text>
                            )}
                            {getPrefered(review.comment as any)}
                        </Box>
                    </HStack>
                ))}
        </VStack>
    );
}
