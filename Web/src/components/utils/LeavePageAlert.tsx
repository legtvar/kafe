import { 
    BoxProps, 
    AlertDialog, 
    AlertDialogBody, 
    AlertDialogContent, 
    AlertDialogFooter, 
    AlertDialogHeader, 
    AlertDialogOverlay, 
    Button, 
} from '@chakra-ui/react';
import { createRef } from 'react';
import { t } from 'i18next';

interface ILeavePageAlertProps extends BoxProps {
    isOpen: boolean;
    onClose: () => void;
    handleConfirm: () => void;
}

export function LeavePageAlert({ isOpen, onClose, handleConfirm }: ILeavePageAlertProps) {
    const cancelRef = createRef<HTMLButtonElement>();
    
    return (
        <AlertDialog isOpen={isOpen} leastDestructiveRef={cancelRef} onClose={onClose}>
            <AlertDialogOverlay>
                <AlertDialogContent>
                    <AlertDialogHeader fontSize="lg" fontWeight="bold">
                        {t('error.leavePageTitle').toString()}
                    </AlertDialogHeader>

                    <AlertDialogBody>{t('error.leavePageWarn').toString()}</AlertDialogBody>

                    <AlertDialogFooter>
                        <Button ref={cancelRef} onClick={onClose}>
                            {t('generic.cancel').toString()}
                        </Button>
                        <Button
                            colorScheme="red"
                            onClick={() => {
                                handleConfirm();
                                onClose();
                            }}
                            ml={3}
                        >
                            {t('generic.ok').toString()}
                        </Button>
                    </AlertDialogFooter>
                </AlertDialogContent>
            </AlertDialogOverlay>
        </AlertDialog>
    )
}