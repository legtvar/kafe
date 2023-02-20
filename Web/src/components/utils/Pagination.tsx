import { HStack, IconButton } from '@chakra-ui/react';
import React from 'react';
import { AiOutlineLeft, AiOutlineRight } from 'react-icons/ai';
import { sequence } from '../../utils/sequence';

interface IPaginationProps<T> {
    data: Array<T>;
    perPage?: number;
    maxPages?: number;
    children: (row: T, index: number) => React.ReactNode;
}

export function Pagination<T>(props: IPaginationProps<T>) {
    const [page, setPage] = React.useState<number>(0);
    let { data, perPage, maxPages, children } = props;
    perPage = perPage || 20;
    maxPages = maxPages || 7;
    const pagesInData = Math.ceil(data.length / perPage);

    if (page >= pagesInData) {
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

    return (
        <>
            <>{currentPage.map(children)}</>
            <HStack my={7} overflowX="auto">
                <IconButton
                    size="md"
                    variant="solid"
                    onClick={() => setPage(Math.max(page - 1, 0))}
                    aria-label={'Previous'}
                    icon={<AiOutlineLeft />}
                    isDisabled={page === 0}
                />
                {visiblePages.map((id) => (
                    <IconButton
                        size="md"
                        variant="solid"
                        colorScheme={id === page ? 'brand' : undefined}
                        onClick={() => setPage(id)}
                        aria-label={`Page ${(id + 1).toString()}`}
                        icon={<>{id + 1}</>}
                    />
                ))}
                <IconButton
                    size="md"
                    variant="solid"
                    onClick={() => setPage(Math.min(page + 1, pagesInData - 1))}
                    aria-label={'Next'}
                    icon={<AiOutlineRight />}
                    isDisabled={page === pagesInData - 1}
                />
            </HStack>
        </>
    );
}
