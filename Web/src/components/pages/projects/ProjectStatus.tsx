import { Text } from '@chakra-ui/react';
import { t } from 'i18next';
import { Project } from '../../../data/Project';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { components } from '../../../schemas/api';
import { HRIB, localizedString } from '../../../schemas/generic';
import { getPrefered } from '../../../utils/preferedLanguage';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { StatusCheck } from './StatusCheck';

interface IProjectStatusProps {
    projectId: HRIB;
}

export function ProjectStatus(props: IProjectStatusProps) {
    const { lighten } = useColorScheme();
    return (
        <AwaitAPI
            request={(api) => api.projects.validationById(props.projectId)}
            error={<Text color={lighten}>Automatizovaná kontrola nebyla spuštěna</Text>}
        >
            {(validation: components['schemas']['ProjectValidationDto']) => (
                <AwaitAPI request={(api) => api.projects.getById(props.projectId)}>
                    {(project: Project) => (
                        <>
                            <StatusGroup stage="info" diagnostics={validation.diagnostics} />
                            <StatusGroup stage="file" diagnostics={validation.diagnostics} />
                            {project.blueprint.requiredReviewers.map((stage, i) => (
                                <StatusGroup stage={stage} reviews={project.reviews} key={i} />
                            ))}
                        </>
                    )}
                </AwaitAPI>
            )}
        </AwaitAPI>
    );
}

interface IStatusGroupProps {
    diagnostics?: components['schemas']['ProjectDiagnosticDto'][];
    reviews?: components['schemas']['ProjectReviewDto'][];
    stage: string;
    useUnset?: boolean;
}

function StatusGroup(props: IStatusGroupProps) {
    if (props.diagnostics) {
        const inStage = props.diagnostics.filter((d) => d.validationStage === props.stage);
        // const worst = inStage.reduce((prev, curr) => [curr.kind, prev].includes("Error") ? "Error" : [curr.kind, prev].includes("Warn") ? "Warn" : [curr.kind, prev].includes("Info") ? "Info" : "Unknown", "Unknown");

        if (inStage.length === 0) {
            return <StatusCheck status="ok">{t(`projectStatus.${props.stage}`).toString()}</StatusCheck>;
        } else {
            return (
                <StatusCheck
                    status={props.useUnset ? 'unknown' : 'nok'}
                    details={
                        props.useUnset ? undefined : (
                            <>
                                {inStage.map((diag, i) => (
                                    <Text key={i}>{getPrefered(diag.message as any as localizedString)}</Text>
                                ))}
                            </>
                        )
                    }
                >
                    {t(`projectStatus.${props.stage}`).toString()}
                </StatusCheck>
            );
        }
    }

    if (props.reviews) {
        const inStage = props.reviews
            .filter((d) => d.reviewerRole === props.stage)
            .sort((a, b) => new Date(b.addedOn).getTime() - new Date(a.addedOn).getTime());

        if (inStage.length === 0) {
            return <StatusCheck status="unknown">{t(`projectStatus.${props.stage}`).toString()}</StatusCheck>;
        } else {
            return (
                <StatusCheck
                    status={inStage[0].kind === 'Accepted' ? 'ok' : 'nok'}
                    details={<Text>{getPrefered(inStage[0].comment as any as localizedString)}</Text>}
                >
                    {t(`projectStatus.${props.stage}`).toString()}
                </StatusCheck>
            );
        }
    }

    return <></>;
}
