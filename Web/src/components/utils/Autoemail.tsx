import { Link } from "@chakra-ui/react";

interface IAutoemailProps {
    children?: string
}

export function Autoemail(props: IAutoemailProps) {
    return <Link href={"mailto:" + props.children ?? "/"}>{props.children}</Link>
}
