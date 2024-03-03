import { useTitle } from "../../utils/useTitle"

interface IWithTitleProps {
    title?: string | null
}

export function WithTitle({title}: IWithTitleProps)
{
    useTitle(title);
    return <></>;
}
