import { Link } from "@chakra-ui/react";

interface IAutolinkProps {
    children?: string
}

export function Autolink(props: IAutolinkProps) {
    return <Link href={props.children ?? "/"}>{props.children}</Link>
}
