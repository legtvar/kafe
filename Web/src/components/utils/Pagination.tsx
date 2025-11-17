import {
    Box,
    Center,
    HStack,
    IconButton,
    Text,
    useColorModeValue,
    useSizes,
    VStack,
} from "@chakra-ui/react";
import { t } from "i18next";
import React from "react";
import {
    AiOutlineDoubleLeft,
    AiOutlineDoubleRight,
    AiOutlineLeft,
    AiOutlineRight,
} from "react-icons/ai";
import { BsFillInboxFill } from "react-icons/bs";
import { sequence } from "../../utils/sequence";

interface IPaginationProps<T> {
    data: Array<T>;
    perPage?: number;
    maxPages?: number;
    children: (row: T, index: number) => React.ReactNode;
}

export function Pagination<T>(props: IPaginationProps<T>) {
    const [page, setPage] = React.useState<number>(0);
    const { data, children } = props;
    let { perPage, maxPages } = props;
    perPage = perPage || 20;
    maxPages = maxPages || 7;
    const pagesInData = Math.ceil(data.length / perPage);

    if (pagesInData === 0) {
        return (
            <Center py={16} color="gray.500">
                <VStack>
                    <Text fontSize="4em">
                        <BsFillInboxFill />
                    </Text>
                    <Text fontSize="18px" mt={3} mb={2}>
                        {t("generic.empty").toString()}
                    </Text>
                </VStack>
            </Center>
        );
    }

    if (page >= pagesInData && page !== 0) {
        setPage(0);
    }

    let min = page - Math.ceil(maxPages / 2) + 1;
    let max = min + maxPages;

    if (pagesInData < maxPages) {
        min = 0;
        max = pagesInData;
    } else {
        if (min < 0) {
            min = 0;
            max = Math.min(pagesInData, min + maxPages);
        }
        if (max > pagesInData) {
            max = pagesInData;
            min = Math.max(0, max - maxPages);
        }
    }

    const visiblePages = sequence(pagesInData).slice(min, max);

    const currentPage = data.slice(page * perPage, (page + 1) * perPage);

    const pagerBgColor = useColorModeValue("gray.100", "gray.800");
    const pagerBorderColor = useColorModeValue("gray.300", "gray.700");
    const pagerPy = 3;

    return (
        <>
            <>{currentPage.map(children)}</>
            {pagesInData > 1 && (
                <>
                    <Box blockSize={10 + 2 * pagerPy}></Box>
                    <HStack
                        py={pagerPy}
                        px={5}
                        mx={-4}
                        overflowX="auto"
                        position={"fixed"}
                        bottom={0}
                        bgColor={pagerBgColor}
                        width="100%"
                        borderTopWidth="1px"
                        borderTopColor={pagerBorderColor}
                    >
                        <IconButton
                            size="md"
                            variant="solid"
                            onClick={() => setPage(0)}
                            aria-label={"First"}
                            icon={<AiOutlineDoubleLeft />}
                            isDisabled={page === 0}
                            key={-2}
                        />
                        <IconButton
                            size="md"
                            variant="solid"
                            onClick={() => setPage(Math.max(page - 1, 0))}
                            aria-label={"Previous"}
                            icon={<AiOutlineLeft />}
                            isDisabled={page === 0}
                            key={-1}
                        />
                        {visiblePages.map((id) => (
                            <IconButton
                                size="md"
                                variant="solid"
                                colorScheme={id === page ? "brand" : undefined}
                                onClick={() => setPage(id)}
                                aria-label={`Page ${(id + 1).toString()}`}
                                icon={<>{id + 1}</>}
                                key={id}
                            />
                        ))}
                        <IconButton
                            size="md"
                            variant="solid"
                            onClick={() =>
                                setPage(Math.min(page + 1, pagesInData - 1))}
                            aria-label={"Next"}
                            icon={<AiOutlineRight />}
                            isDisabled={page === pagesInData - 1}
                            key={-11}
                        />
                        <IconButton
                            size="md"
                            variant="solid"
                            onClick={() => setPage(pagesInData - 1)}
                            aria-label={"Last"}
                            icon={<AiOutlineDoubleRight />}
                            isDisabled={page === pagesInData - 1}
                            key={-12}
                        />
                    </HStack>
                </>
            )}
        </>
    );
}
