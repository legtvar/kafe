import { Box, HStack, Text, useColorModeValue, useToast } from "@chakra-ui/react";
import { t } from "i18next";
import { useState } from "react";
import { API, ApiResponse } from "../../api/API";
import { useApi } from "../../hooks/Caffeine";
import { components } from "../../schemas/api";
import { HRIB } from "../../schemas/generic";

type SendAPIStatus = "ready" | "sending" | "error" | "ok";

function getErrorDescription(
    problemDetails: components["schemas"]["KafeProblemDetails"],
) {
    let children = [];
    if (problemDetails.title != null) {
        children.push(<Text>{problemDetails.title}</Text>);
    }

    if (problemDetails.errors != null) {
        for (let i = 0; i < problemDetails.errors.length; ++i) {
            const error = problemDetails.errors[i];
            const errorChildren = [];
            if (error.arguments["parameter"]) {
                errorChildren.push(
                    <Text fontWeight={"bold"}>{error.arguments["parameter"] as string}:</Text>,
                );
            }

            errorChildren.push(<Text>{error.message}</Text>);
            children.push(<HStack>{errorChildren}</HStack>);
        }
    }

    return <>{children}</>;
}

export interface ISendAPIProps<T> {
    request: (api: API, data: T) => Promise<ApiResponse<HRIB>>;
    value: T;
    children: (
        onSubmit: () => void,
        status: SendAPIStatus,
        error: components["schemas"]["KafeProblemDetails"] | null,
    ) => JSX.Element;
    onSubmited: (id: HRIB) => void;
    repeatable?: boolean;
}

export function SendAPI<T>(
    { request, value, children, onSubmited, repeatable }: ISendAPIProps<T>,
) {
    const api = useApi();
    const [status, setStatus] = useState<SendAPIStatus>("ready");
    const [error, setError] = useState<
        components["schemas"]["KafeProblemDetails"] | null
    >(null);
    const border = useColorModeValue("orange.300", "orange.800");
    const bg = useColorModeValue("orange.200", "orange.900");
    const toast = useToast();

    const onSubmit = async () => {
        if (!(status === "ready" || status === "error")) return;
        setStatus("sending");
        const result = await request(api, value);

        if (result.status === 200) {
            if (repeatable) {
                setStatus("ready");
            } else {
                setStatus("ok");
            }
            onSubmited(result.data);
        } else {
            setStatus("error");
            console.log(result);
            setError(result.error);
            toast({
                title: t("sendApi.error").toString(),
                description: getErrorDescription(result.error),
                status: "error",
                duration: 9000,
                isClosable: true,
            });
        }
    };

    return (
        <Box>
            {
                /* {status === 'error' && (
                <>
                    <Box borderRadius={'lg'} border="1px" borderColor={border} bg={bg} px={5} py={4} mb={8}>
                        <Heading as="h4" fontSize="lg" pb={4}>
                            {t('sendApi.error').toString()}
                        </Heading>
                        {typeof error === 'string' && (
                            <Text>
                                {(error as string)
                                    .split('\n')[0]
                                    .replace('System.ArgumentException: ', '')
                                    .replace(" (Parameter 'dto')", '')}
                            </Text>
                        )}
                        {error?.errors ? (
                            Object.entries(error.errors).map(([field, problem]: any, i) => (
                                <Text key={i}>
                                    <strong>{field}: </strong>
                                    {problem}
                                </Text>
                            ))
                        ) : (
                            <Text>{error!.title}</Text>
                        )}
                    </Box>
                    <Spacer />
                </>
            )} */
            }
            {children(onSubmit, status, error)}
        </Box>
    );
}
