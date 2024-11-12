import { Button, Flex, FormControl, Highlight, Icon, Input, Text, useColorModeValue } from '@chakra-ui/react';
import { t } from 'i18next';
import { useState } from 'react';
import { IoAdd, IoPeopleOutline } from 'react-icons/io5';
import { Link } from 'react-router-dom';
import { useOrganizations } from '../../../hooks/Caffeine';
import { useColorScheme, useHighlightStyle } from '../../../hooks/useColorScheme';
import { fulltextFilter } from '../../../utils/fulltextFilter';
import { useTitle } from '../../../utils/useTitle';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { Pagination } from '../../utils/Pagination';

interface IOrganizationsProps {}

export function Organizations(props: IOrganizationsProps) {
    const borderColor = useColorModeValue('gray.300', 'gray.700');
    const hoverColor = useColorModeValue('gray.200', 'gray.700');
    useTitle(t('title.organizations'));

    const { border, bg } = useColorScheme();
    const highlightStyle = useHighlightStyle();
    const [filter, setFilter] = useState('');

    const { organizations } = useOrganizations();

    return (
        <OutletOrChildren>
            <>
                <Flex direction="row" mx={-4} px={4} pb={4} borderBottomWidth="1px" borderBottomColor={borderColor}>
                    <FormControl>
                        <Input
                            type="text"
                            borderColor={border}
                            bg={bg}
                            placeholder={`${t('organizations.search').toString()}`}
                            value={filter}
                            onChange={(event) => setFilter(event.target.value.toLowerCase())}
                        />
                    </FormControl>
                    <Link to="create">
                        <Button ml={4} leftIcon={<IoAdd />} colorScheme="blue">
                            {t('organizations.create').toString()}
                        </Button>
                    </Link>
                </Flex>
                <Flex direction="column" w="full">
                    <Pagination data={organizations.filter((group) => fulltextFilter(group.name, filter))}>
                        {(organization, i) => (
                            <Link to={organization.id} key={i}>
                                <Flex
                                    direction={{
                                        base: 'column',
                                        md: 'row',
                                    }}
                                    mx={-4}
                                    py={7}
                                    px={8}
                                    borderBottomWidth="1px"
                                    borderBottomColor={borderColor}
                                    align={{
                                        base: 'start',
                                        md: 'center',
                                    }}
                                    cursor="pointer"
                                    _hover={{
                                        background: hoverColor,
                                    }}
                                >
                                    <Icon as={IoPeopleOutline} mb="auto" mr={3} my={1} fontSize="xl" />
                                    <Flex direction="column" flex="1">
                                        <Text>
                                            <Highlight styles={highlightStyle} query={filter}>
                                                {organization.getName()}
                                            </Highlight>
                                        </Text>
                                    </Flex>
                                </Flex>
                            </Link>
                        )}
                    </Pagination>
                </Flex>
            </>
        </OutletOrChildren>
    );
}
