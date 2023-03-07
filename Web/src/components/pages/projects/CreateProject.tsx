import {
    Accordion,
    AccordionButton,
    AccordionIcon,
    AccordionItem,
    AccordionPanel,
    Box,
    ListItem,
    Stack,
    Text,
    UnorderedList,
} from '@chakra-ui/react';
import { t } from 'i18next';
import { useParams } from 'react-router-dom';
import { Group } from '../../../data/Group';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { Status } from '../../utils/Status';
import { ProjectBasicInfo } from './create/ProjectBasicInfo';

interface ICreateProjectProps {}

export function CreateProject(props: ICreateProjectProps) {
    const { id } = useParams();

    if (!id) {
        return <Status statusCode={404} embeded />;
    }

    return (
        <OutletOrChildren>
            <AwaitAPI request={(api) => api.groups.getById(id)} error={<Status statusCode={404} embeded />}>
                {(group: Group) => (
                    <Stack spacing={4} m={6} direction="column">
                        <Box fontSize="xl" as="h2" lineHeight="tight" color="gray.500" isTruncated>
                            {t('createProject.title').toString()}
                        </Box>
                        <Box fontSize="4xl" fontWeight="semibold" as="h2" lineHeight="tight" isTruncated>
                            {group.getName()}
                        </Box>
                        <Box>
                            <Text pb={2}>
                                Chceš točit filmy? Chceš se ukázat? Chceš ukázat svůj úhel pohledu? Máš nápad a
                                potřebuješ nakopnout?
                            </Text>
                            <Text pb={2}>
                                Tak tohle je tvoje jedinečná příležitost! Natoč krátký film, přihlaš ho na 22. Filmový
                                festival Fakulty informatiky Masarykovy univerzity a vyhraj. Deadline? 19. dubna 2022,
                                tak neotálej!
                            </Text>
                            <Text pb={2}>
                                S námi můžeš ukázat svoji tvorbu před stovkami lidí a pokud tvůj snímek zaujme porotu
                                nebo diváky, budeš oceněn i cenami.
                            </Text>
                            <Text pb={2}>Už jsi náš?</Text>

                            <Accordion allowToggle mt={8} mb={8} mx={-4}>
                                <AccordionItem>
                                    <AccordionButton>
                                        <Box as="span" flex="1" textAlign="left">
                                            <Text pb={2} fontSize="2xl" fontWeight="semibold" as="h3">
                                                Propozice {group.getName()}
                                            </Text>
                                        </Box>
                                        <AccordionIcon />
                                    </AccordionButton>
                                    <AccordionPanel pb={4}>
                                        <Text pb={2}>
                                            Filmový festival Fakulty informatiky Masarykovy univerzity je festival
                                            krátkých amatérských filmů. Festivalový večer se bude konat na Fakultě
                                            informatiky MU, v Univerzitním kině Scala a zároveň se bude streamovat pro
                                            diváky na YouTube a to v pátek 20. května 2022 od 19 hodin.
                                        </Text>

                                        <Text pb={2} pt={8} fontSize="xl" fontWeight="semibold" as="h4">
                                            Podmínky pro přijetí filmu na 22. Filmový Festival FI MU
                                        </Text>

                                        <UnorderedList>
                                            <ListItem>
                                                Do soutěže mohou být přijata amatérská audiovizuální díla libovolného
                                                žánru i formy zpracování (hraná, animovaná) v délce do 8 minut
                                            </ListItem>
                                            <ListItem>
                                                Tvůrci filmu nesmí být profesionálové v tvorbě audiovizuálních děl
                                            </ListItem>
                                            <ListItem>Snímek nesmí být starší než 3 roky</ListItem>
                                            <ListItem>Snímek nesmí porušovat žádná autorská práva</ListItem>
                                            <ListItem>
                                                Film může přihlásit pouze jeden z jeho autorů, a to se souhlasem všech
                                                spoluautorů díla
                                            </ListItem>
                                            <ListItem>Přihlášení je zdarma.</ListItem>
                                        </UnorderedList>

                                        <Text pb={2} pt={8} fontSize="xl" fontWeight="semibold" as="h4">
                                            Soutěžní snímky budou hodnoceny dvěma způsoby
                                        </Text>

                                        <Text pb={2}>Hlasováním odborné poroty</Text>
                                        <Text pb={2}>Diváckou anketou</Text>

                                        <Text pb={2}>
                                            Snímky je nutné přihlásit do 19. dubna 2022 pomocí online formuláře přes
                                            odkaz výše. Každý příspěvek musí mít výstižný název. Součástí přihlášky musí
                                            být též doprovodný text se stručnou anotací snímku. Pořadatelé festivalu si
                                            vyhrazují právo bez předání k posouzení Programovému výboru vyloučit
                                            příspěvky, které nesplňují výše uvedené podmínky, nebo příspěvky, které jsou
                                            obsahově v rozporu s českou legislativou. Každý autor/autorský kolektiv může
                                            přihlásit na festival maximálně tři samostatné snímky. Výběr snímků k
                                            projekci a dramaturgii večera určí ředitel festivalu s vyučujícími předmětu
                                            PV113 Produkce filmového díla. Tato interní porota není shodná s odbornou
                                            porotou, která finálně určí vítězné pozice. Na zařazení do programu
                                            festivalu není právní nárok.
                                        </Text>
                                        <Text pb={2}>
                                            Autoři budou o přijetí či nepřijetí díla k projekci na festivalu informování
                                            pomocí kontaktů uvedených v přihlášce. Snímky vybrané Programovým výborem
                                            budou během 22. Filmového festivalu Fakulty informatiky Masarykovy
                                            univerzity streamovány na YouTube a promítány na plátnách v Brně.
                                        </Text>
                                        <Text pb={2}>
                                            Na výhru v soutěži nemá soutěžící právní nárok a nepeněžní ceny nelze ani
                                            alternativně plnit či proměnit v penězích. Ceny budou vydány na základě
                                            oboustranné dohody autora s pořadateli 22. Filmového festivalu Fakulty
                                            informatiky MU.
                                        </Text>
                                        <Text pb={2}>
                                            Přihlášením audiovizuálního díla (filmu) přihlašující prohlašuje, že
                                            disponuje právem dílo takto užít a že disponuje oprávněními k dílům ve filmu
                                            užitým.
                                        </Text>
                                        <Text pb={2}>
                                            Přihlášením filmu přihlašující poskytuje pořadateli bezplatnou licenci k
                                            promítnutí díla v rámci Filmového festivalu Fakulty informatiky MU.
                                        </Text>
                                        <Text pb={2}>
                                            Autor přihlášením snímku dále MU poskytuje nevýhradní bezplatnou licenci k
                                            uchování rozmnoženiny k případnému promítání díla v rámci výuky a propagaci
                                            FFFI MU a k neveřejnému promítání pro akademickou obec MU. Územní a
                                            množstevní rozsah licence je neomezený. Oprávnění jsou udělována na celou
                                            dobu trvání příslušných práv.
                                        </Text>

                                        <Text pb={2} pt={8} fontSize="xl" fontWeight="semibold" as="h4">
                                            Doporučení
                                        </Text>

                                        <Text pb={2}>
                                            Včas ověřte, že platforma YouTube nedetekuje ve vašem filmu porušení
                                            autorských práv.
                                        </Text>
                                        <Text pb={2}>
                                            Pokud je potřeba platformě YouTube doložit, že držíte licenční práva na
                                            použitý materiál, kontaktujte nás na festival-tech@fi.muni.cz.
                                        </Text>
                                    </AccordionPanel>
                                </AccordionItem>
                            </Accordion>
                        </Box>

                        <ProjectBasicInfo groupId={id} />

                        {/*
                            - Genre
                            - Name
                            - Description
                            ? Visibility
                            ? ReleaseDate
                            Crew
                            Cast
                            Artifacts
                        */}
                    </Stack>
                )}
            </AwaitAPI>
        </OutletOrChildren>
    );
}
