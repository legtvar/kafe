import { Box, BoxProps } from '@chakra-ui/react';
import { Trans } from 'react-i18next';
import styled, { keyframes } from 'styled-components';

interface IFooterProps extends BoxProps {}

const pulseAnimation = keyframes`
    10% { transform: scale(1.3); }
`;

const Heart = styled.span`
    animation: ${pulseAnimation} 1s infinite;
    display: inline-block;
`;

export function Footer(props: IFooterProps) {
    return (
        <Box
            className="kafe-layout-footer"
            pb="3"
            pt="6"
            textAlign="center"
            fontSize="smaller"
            opacity={0.3}
            transition="opacity 0.2s linear"
            _hover={{
                opacity: 1,
            }}
            {...props}
        >
            <span className="kafe-footer-content">
                <Trans i18nKey="layout.footer.copy">
                    Created with <Heart>❤️</Heart> and ☕ by LeGTVaR.
                </Trans>
                <br />
                &copy; 2022 - {new Date().getFullYear()}
            </span>
        </Box>
    );
}
