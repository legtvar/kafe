import { Link, useNavigate } from 'react-router-dom';
import { LeavePageAlert } from './LeavePageAlert';
import { useDisclosure } from '@chakra-ui/react';

interface ILeavePageAlertProps {
    alertCondition: boolean;
    destPath: string;
    handleConfirm: () => void;
    children: React.ReactNode;
}

export function ConfirmExitLink({ alertCondition, destPath, handleConfirm, children }: ILeavePageAlertProps) {
    const {
        isOpen,
        onOpen,
        onClose
    } = useDisclosure()
    
    const navigate = useNavigate();
    
    return (
        <Link to={destPath}
            onClick={(e) => {
                if (alertCondition) {
                    e.preventDefault();
                    onOpen();
                }
                else {
                    handleConfirm();
                }
            }}
        >
        <LeavePageAlert
            isOpen={isOpen}
            onClose={onClose}
            handleConfirm={() => {
                navigate(destPath);
                handleConfirm();
            }}
        />
        {children}
        </Link>
    )
}