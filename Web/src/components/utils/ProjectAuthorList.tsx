import { Button, CloseButton, HStack, IconButton, SimpleGrid, Text, Tooltip, VStack } from '@chakra-ui/react';
import { useCallback, useState } from 'react';
import { API } from '../../api/API';
import { Author } from '../../data/Author';
import { components } from '../../schemas/api';
import { HRIB } from '../../schemas/generic';
import { AwaitAPI } from './AwaitAPI';
import { KafeAvatar } from './KafeAvatar';
import { t } from 'i18next';
import crewRoleManager from '@/utils/managers/crewRolesManager';
import { AuthorFlowDialog } from './Authors/AuthorFlowDialog';
import { AiOutlineEdit, AiOutlineClose } from 'react-icons/ai';
import { BsPeople } from 'react-icons/bs';

interface IProjectAuthorListProps {
    authors: components['schemas']['ProjectAuthorDto'][];
    editable?: boolean;
    requestDetails?: boolean;
    onRemove?: (id: HRIB) => void;
    onRoleChange?: (id: HRIB, roles: string[]) => void;
    isDropdownCrewList?: boolean;
}

function ProjectAuthorListAwaiter(
    props: Omit<IProjectAuthorListProps, 'authors'> & { author: components['schemas']['ProjectAuthorDto'] },
) {
    const request = useCallback((api: API) => api.authors.getById(props.author.id), [props.author]);
    return props.requestDetails ? (
        <AwaitAPI key={props.author.id} request={request}>
            {(data: Author) => <SimpleProjectAuthorList {...props} author={{ ...props.author, ...data }} />}
        </AwaitAPI>
    ) : (
        <SimpleProjectAuthorList {...props} author={props.author} />
    );
}

export function ProjectAuthorList(props: IProjectAuthorListProps) {
    return (
        <SimpleGrid alignItems="start" spacing={4} columns={{ base: 1, md: 2, lg: 3, xl: 4 }}>
            {props.authors.map((author, i) => (
                <ProjectAuthorListAwaiter {...props} author={author} key={i} />
            ))}
        </SimpleGrid>
    );
}

function SimpleProjectAuthorList({
    author,
    onRemove,
    onRoleChange,
    editable,
    isDropdownCrewList,
}: { author: components['schemas']['ProjectAuthorDto'] & Partial<Author> } & Omit<IProjectAuthorListProps, 'authors'>) {
    const [editedAuthor, setEditedAuthor] = useState<Author | null>(null);
    const [editedRoles, setEditedRoles] = useState<string[] | null>(null);
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [isRoleDialogOpen, setIsRoleDialogOpen] = useState(false);
    const displayedAuthor = editedAuthor ? { ...author, ...editedAuthor } : author;
    const displayedRoles = editedRoles ?? displayedAuthor.roles;
    const canEditDetails = editable === true && displayedAuthor.userPermissions?.includes('write') === true;

    return (
        <HStack alignItems="start">
            <KafeAvatar size={'md'} person={displayedAuthor} />
            <VStack alignItems="start" pl={2} spacing={0}>
                {canEditDetails ? (
                    <Text data-testid="project-author-name" fontWeight="bolder">{displayedAuthor.name}</Text>
                ) : (
                    <Button
                        data-testid="project-author-name"
                        variant="link"
                        fontWeight="bolder"
                        onClick={() => setIsDialogOpen(true)}
                    >
                        {displayedAuthor.name}
                    </Button>
                )}
                <Text data-testid="project-author-roles" color="gray.500">
                    {(isDropdownCrewList && displayedRoles.every((role) => crewRoleManager.tags.has(role))
                        ? displayedRoles.map((role) => t(`createProject.fields.crewRoles.${role}`))
                        : displayedRoles
                    ).join(', ')}
                </Text>
            </VStack>
            {editable && (
                <HStack spacing={1}>
                    <Tooltip label={t('authorSelect.editDetails').toString()}>
                        <IconButton
                            data-testid="project-author-edit-details"
                            aria-label={t('authorSelect.editDetails').toString()}
                            icon={<AiOutlineEdit />}
                            size="sm"
                            isDisabled={!canEditDetails}
                            onClick={() => setIsDialogOpen(true)}
                        />
                    </Tooltip>
                    <Tooltip label={t('authorSelect.editRoles').toString()}>
                        <IconButton
                            data-testid="project-author-edit-roles"
                            aria-label={t('authorSelect.editRoles').toString()}
                            icon={<BsPeople />}
                            size="sm"
                            onClick={() => setIsRoleDialogOpen(true)}
                        />
                    </Tooltip>
                    <Tooltip label={t('authorSelect.removeAuthor').toString()}>
                        <IconButton
                            aria-label={t('authorSelect.removeAuthor').toString()}
                            icon={<AiOutlineClose />}
                            size="sm"
                            onClick={() => onRemove && onRemove(author.id)}
                        />
                    </Tooltip>
                </HStack>
            )}

            {isDialogOpen && (
                <AuthorFlowDialog
                    isOpen={isDialogOpen}
                    author={
                        new Author({
                            ...displayedAuthor,
                            globalPermissions: displayedAuthor.globalPermissions ?? 'none',
                        })
                    }
                    canEdit={canEditDetails}
                    onClose={() => setIsDialogOpen(false)}
                    onSaved={setEditedAuthor}
                />
            )}
            {isRoleDialogOpen && (
                <AuthorFlowDialog
                    isOpen={isRoleDialogOpen}
                    initialStatus="role"
                    roles={displayedRoles}
                    isDropdownCrewSelect={isDropdownCrewList}
                    onClose={() => setIsRoleDialogOpen(false)}
                    onRolesSaved={(roles) => {
                        setEditedRoles(roles);
                        onRoleChange?.(author.id, roles);
                    }}
                />
            )}
        </HStack>
    );
}
