import { Text } from '@chakra-ui/react';
import { t } from 'i18next';
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
            {(data: components['schemas']['ProjectValidationDto']) => (
                <>
                    <StatusGroup stage="info" diagnostics={data.diagnostics} />
                    <StatusGroup stage="file" diagnostics={data.diagnostics} />
                    <StatusGroup stage="tech-review" diagnostics={data.diagnostics} useUnset />
                    <StatusGroup stage="visual-review" diagnostics={data.diagnostics} useUnset />
                    <StatusGroup stage="dramaturgy-review" diagnostics={data.diagnostics} useUnset />

                    {/* <StatusCheck status="ok">Základní informace jsou dostatečně vyplněny</StatusCheck>
                    <StatusCheck status="ok">Soubory jsou nahrány</StatusCheck>
                    <StatusCheck
                        status="nok"
                        details={
                            <>
                                Do ullamco irure ut dolore. Cillum dolor consectetur sint nulla ut ea labore cupidatat.
                                Aliqua consectetur minim nulla exercitation do fugiat nisi ex sunt ea elit anim esse ut.
                                Veniam eiusmod est pariatur cupidatat labore. Ut ullamco nulla ipsum irure qui laboris
                                minim exercitation tempor dolore consequat incididunt. Enim commodo non adipisicing ex.
                            </>
                        }
                    >
                        Soubory prošly automatizovanou kontrolou
                    </StatusCheck>
                    <StatusCheck status="unknown">Projekt prošel kontrolou technikem</StatusCheck>
                    <StatusCheck status="unknown">Projekt prošel kontrolou grafikem</StatusCheck>
                    <StatusCheck status="unknown">Projekt byl přijat</StatusCheck> */}
                </>
            )}
        </AwaitAPI>
    );
}

interface IStatusGroupProps {
    diagnostics: components['schemas']['ProjectDiagnosticDto'][];
    stage: string;
    useUnset?: boolean;
}

function StatusGroup(props: IStatusGroupProps) {
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
                            {inStage.map((diag) => (
                                <Text>{getPrefered(diag.message as any as localizedString)}</Text>
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
