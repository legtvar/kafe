import { Box, HStack, Text, useColorModeValue, VStack } from '@chakra-ui/react';
import { t } from 'i18next';
import { BsCheckCircleFill, BsFillExclamationCircleFill, BsFillXCircleFill, BsCircle } from 'react-icons/bs';
import { Project } from '../../../data/Project';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { getPrefered } from '../../../utils/preferedLanguage';
import { DateTime } from 'luxon';
import { User } from '../../../data/User';
import { AwaitAPI } from '../../utils/AwaitAPI';

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
                        <ReviewIcon kind={review.kind} />
                        <Box>
                            {review.reviewerId ?
                                <AwaitAPI
                                    request={(api) => api.accounts.info.getById(review.reviewerId!)}
                                    error={() => <Text fontWeight="bold">{t(`project.admin.role.${review.reviewerRole}`).toString()}</Text>}
                                >
                                    {(owner: User) => {
                                        return (
                                            <Text fontWeight="bold"> {owner.name && owner.name} </Text>
                                        );
                                    }}
                                </AwaitAPI> :
                                <Text fontWeight="bold">{t(`project.admin.role.${review.reviewerRole}`).toString()}</Text>
                            }
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

export function ReviewIcon({ kind }: { kind: string }) {
    return (
        <HStack spacing={4} alignItems="center">
            <Box fontSize="2.5rem">
                {kind === 'accepted' ? (
                    <Text color="green.500" title={t('project.admin.kind.accepted')}>
                        <BsCheckCircleFill />
                    </Text>
                ) : kind === 'needsRevision' ? (
                    <Text color="yellow.500" title={t('project.admin.kind.needsRevision')}>
                        <BsFillExclamationCircleFill />
                    </Text>
                ) : kind === 'rejected' ? (
                    <Text color="red.500" title={t('project.admin.kind.rejected')}>
                        <BsFillXCircleFill />
                    </Text>
                ) : (
                    <Text color={useColorModeValue('gray.300', 'gray.600')} title={t('project.admin.kind.notReviewed')}>
                        <BsCircle />
                    </Text>
                )}
            </Box>
        </HStack>
    );
}