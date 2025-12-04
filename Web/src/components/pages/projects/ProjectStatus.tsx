import { Box, Flex, Text, VStack } from '@chakra-ui/react';
import { t } from 'i18next';
import { useCallback } from 'react';
import { API } from '../../../api/API';
import { Project } from '../../../data/Project';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { components } from '../../../schemas/api';
import { HRIB, localizedString } from '../../../schemas/generic';
import { getPrefered } from '../../../utils/preferedLanguage';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { StatusCheck } from './StatusCheck';
import { IoCheckbox, IoCloseCircleSharp, IoHelpCircleOutline } from 'react-icons/io5';
import { useOrganizations } from '../../../hooks/Caffeine';

interface IProjectStatusProps {
    projectId: HRIB;
}

export function ProjectStatus(props: IProjectStatusProps) {
    const { lighten } = useColorScheme();
    const { currentOrganization } = useOrganizations();
    const isMate = currentOrganization?.id === 'mate-fimuni';

    const getProject = useCallback((api: API) => api.projects.getById(props.projectId), [props.projectId]);

    return (
        <AwaitAPI
            request={useCallback((api) => api.projects.validationById(props.projectId), [props.projectId])}
            error={<Text color={lighten}>Automatizovaná kontrola nebyla spuštěna</Text>}
        >
            {(validation: components['schemas']['ProjectValidationDto']) => (
                <AwaitAPI request={getProject}>
                    {(project: Project) => (
                        <>
                            <StatusGroup stage="info" diagnostics={validation.diagnostics} />
                            <StatusGroup stage="file" diagnostics={validation.diagnostics} />
                            {project.blueprint.requiredReviewers.map((stage, i) => (
                                <StatusGroup stage={stage} reviews={project.reviews} key={i} />
                            ))}
                            {isMate && <StatusGroup stage="pigeons-test" diagnostics={validation.diagnostics} />}
                            {isMate && <StatusGroup
                                stage="pigeons-review"
                                reviews={project.reviews
                                    .slice()
                                    .sort((a, b) => new Date(b.addedOn).getTime() - new Date(a.addedOn).getTime())
                                }
                            />}
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

export function StatusGroup(props: IStatusGroupProps) {
    if (props.diagnostics) {
        const { border, bg } = useColorScheme();
        const inStage = props.diagnostics.filter((d) => d.validationStage === props.stage);
        // const worst = inStage.reduce((prev, curr) => [curr.kind, prev].includes("Error") ? "Error" : [curr.kind, prev].includes("Warn") ? "Warn" : [curr.kind, prev].includes("Info") ? "Info" : "Unknown", "Unknown");
        if (props.stage ==='pigeons-test') {
            const status = inStage.length === 0 ? 'unknown' : inStage.find(diag => diag.kind === 'error') || inStage.find(diag => diag.kind === 'warning') ? 'nok' : 'ok';
            if (inStage.length === 0) {
                return <StatusCheck status={status}>{t(`projectStatus.${props.stage}.unknown`).toString()}</StatusCheck>;
            } else {
                return (
                    <StatusCheck
                        status={status}
                        details={
                            <>
                            <Flex
                                direction={{ base: "column", md: "row" }}
                                align="stretch"
                                wrap="wrap"
                                gap={8}
                                >
                                <Flex direction="column" flex="3" minW="450px">
                                    {inStage
                                    .filter((diag) => diag.kind === "info")
                                    .map((diag, i) => (
                                        <Text key={i}>
                                            <IoCheckbox style={{ display: "inline", marginRight: 4, fontSize: 32, verticalAlign: "text-bottom" }} />
                                            <span style={{ position: "relative", top: "-5px" }}>
                                                {getPrefered(diag.message as any as localizedString)}
                                            </span>
                                        </Text>
                                    ))}

                                    {inStage
                                    .filter((diag) => diag.kind === "warning")
                                    .map((diag, i) => (
                                        <Text key={i} color="red.400">
                                        <IoHelpCircleOutline style={{ display: "inline", marginRight: 4, fontSize: '32px' }} />
                                        <span style={{ position: "relative", top: "-5px" }}>
                                            {getPrefered(diag.message as any as localizedString)}
                                        </span>
                                        </Text>
                                    ))}

                                    {inStage
                                    .filter((diag) => diag.kind === "error")
                                    .map((diag, i) => (
                                        <Text key={i} color="red.400">
                                        <IoCloseCircleSharp style={{ display: "inline", marginRight: 4, fontSize: '32px' }} />
                                        <span style={{ position: "relative", top: "-5px" }}>
                                            {getPrefered(diag.message as any as localizedString)}
                                        </span>
                                        </Text>
                                    ))}
                                </Flex>

                                <Flex
                                    borderColor={border}
                                    bg={bg}
                                    borderWidth={1}
                                    borderRadius="md"
                                    align="center"
                                    justify="center"
                                    flex={{ base: "none", md: "1" }}
                                    p={4}
                                >
                                    <img
                                        src={`/pigeons-img/${status}.png`}
                                        alt="Status illustration"
                                        style={{
                                            objectFit: "contain",
                                            height: "auto",
                                            maxWidth: "250px",
                                        }}
                                    />
                                </Flex>
                            </Flex>

                            </>
                        }
                >
                    {t(`projectStatus.${props.stage}.${status}`).toString()}
                </StatusCheck>
            );
        }}
        if (inStage.length === 0) {
            return <StatusCheck status="ok">{t(`projectStatus.${props.stage}.ok`).toString()}</StatusCheck>;
        } else {
            return (
                <StatusCheck
                    status={props.useUnset ? 'unknown' : 'nok'}
                    details={
                        props.useUnset ? undefined : (
                            <>
                                {props.stage === 'file' && (
                                    <Text key={-1} pb={4} fontStyle="italic">
                                        {t('projectStatus.disclaimer').toString()}
                                    </Text>
                                )}
                                {inStage.map((diag, i) => (
                                    <Text key={i}>{getPrefered(diag.message as any as localizedString)}</Text>
                                ))}
                            </>
                        )
                    }
                >
                    {t(`projectStatus.${props.stage}.nok`).toString()}
                </StatusCheck>
            );
        }
    }

    if (props.reviews) {
        let inStage: components['schemas']['ProjectReviewDto'][] = [];
        if (props.stage === 'pigeons-review') {
            inStage = props.reviews
                .sort((a, b) => new Date(b.addedOn).getTime() - new Date(a.addedOn).getTime());
        } else {
            inStage = props.reviews
                .filter((d) => d.reviewerRole === props.stage)
                .sort((a, b) => new Date(b.addedOn).getTime() - new Date(a.addedOn).getTime());
        }

        if (inStage.length === 0) {
            return <StatusCheck status="unknown">{t(`projectStatus.${props.stage}.unknown`).toString()}</StatusCheck>;
        } else {
            return (
                <StatusCheck
                    status={inStage[0].kind === 'accepted' ? 'ok' : 'nok'}
                    details={<Text>{getPrefered(inStage[0].comment as any as localizedString)}</Text>}
                >
                    {t(`projectStatus.${props.stage}.` + (inStage[0].kind === 'accepted' ? 'ok' : 'nok')).toString()}
                </StatusCheck>
            );
        }
    }

    return <></>;
}


interface IProjectStatusMiniProps {
    project: Project;
    stage?: string;
}

export function ProjectStatusMini(props: IProjectStatusMiniProps) {
    const { lighten } = useColorScheme();
    if (props.stage === undefined) {
        return;
    }

    return (
        <AwaitAPI
            request={useCallback((api) => api.projects.validationById(props.project.id), [props.project.id])}
            error={<></>}
        >
            {(validation: components['schemas']['ProjectValidationDto']) => (
                <Flex direction={{ base: "row" }}>
                    <StatusGroupMini stage={props.stage!} diagnostics={validation.diagnostics} />
                    <StatusGroupMini stage={props.stage!} latestReviewKind={props.project.latestReviewKind} />
                </Flex>
            )}
        </AwaitAPI>
    );
}

interface IStatusGroupProps {
    diagnostics?: components['schemas']['ProjectDiagnosticDto'][];
    latestReviewKind?: components['schemas']['ReviewKind'];
    stage: string;
    useUnset?: boolean;
}

export function StatusGroupMini(props: IStatusGroupProps) {
    if (props.diagnostics) {
        const inStage = props.diagnostics.filter((d) => d.validationStage === props.stage);
        const status = inStage.length === 0 ? 'unknown' : inStage.find(diag => diag.kind === 'error') || inStage.find(diag => diag.kind === 'warning') ? 'nok' : 'ok';

        if (props.stage ==='pigeons-test') {
            return <StatusCheck status={status}><></></StatusCheck>;
        }
    }
    if (props.latestReviewKind) {
        return (
            <ReviewIcon kind={props.latestReviewKind} />
        )   
    }
}
