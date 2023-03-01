import { Navigate, useParams } from 'react-router-dom';
import { Status } from '../utils/Status';

interface IGoRedirectProps {}

export function GoRedirect(props: IGoRedirectProps) {
    const { slug } = useParams();

    console.log(`Slug id: "${slug}"`);

    if (!slug) {
        return <Status statusCode={404} embeded />;
    }

    // TODO: pull from the endpoint

    switch (slug) {
        case 'fffi2023':
            return <Navigate to="/auth/groups/cdrXLt_pOwb/create" />;

        default:
            return <Navigate to="/" />;
    }
}
