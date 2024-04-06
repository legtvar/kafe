import { Link } from '@chakra-ui/react';

interface IAutolinkProps {
    children?: string;
}

export function Autolink(props: IAutolinkProps) {
    return (
        <Link
            href={props.children ?? '/'}
            overflow="clip"
            textOverflow="ellipsis"
            whiteSpace="nowrap"
            display="inline-block"
            maxWidth="100%"
        >
            {props.children}
        </Link>
    );
}
