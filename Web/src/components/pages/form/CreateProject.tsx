import {
    Accordion,
    AccordionButton,
    AccordionIcon,
    AccordionItem,
    AccordionPanel,
    Box,
    FormControl,
    FormLabel,
    Input,
    ListItem,
    Stack,
    Text,
    UnorderedList,
} from '@chakra-ui/react';
import { t } from 'i18next';
import { useParams } from 'react-router-dom';
import { Group } from '../../../data/Group';
import { useColorScheme } from '../../../hooks/useColorScheme';
import { AwaitAPI } from '../../utils/AwaitAPI';
import { OutletOrChildren } from '../../utils/OutletOrChildren';
import { Status } from '../../utils/Status';
import { TextareaLimited } from '../../utils/TextareaLimited';
import { Upload } from '../../utils/Upload';

interface ICreateProjectProps {}

const tempProjectId = '0INB9KB5bhz';

export function CreateProject(props: ICreateProjectProps) {
    const { id } = useParams();
    const { border, bg } = useColorScheme();

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

                        <Stack spacing={8} direction="column">
                            <FormControl>
                                <FormLabel>{t('createProject.fields.name').toString()}</FormLabel>
                                <Stack direction={{ base: 'column', md: 'row' }}>
                                    <FormControl id="name.cs">
                                        <Input
                                            type="text"
                                            borderColor={border}
                                            bg={bg}
                                            placeholder={`${t('createProject.fields.name').toString()} ${t(
                                                'createProject.language.cs',
                                            )}`}
                                        />
                                    </FormControl>

                                    <FormControl id="name.en">
                                        <Input
                                            type="text"
                                            borderColor={border}
                                            bg={bg}
                                            placeholder={`${t('createProject.fields.name').toString()} ${t(
                                                'createProject.language.en',
                                            )}`}
                                        />
                                    </FormControl>
                                </Stack>
                            </FormControl>

                            <FormControl>
                                <FormLabel>{t('createProject.fields.genere').toString()}</FormLabel>
                                <Stack direction={{ base: 'column', md: 'row' }}>
                                    <FormControl id="name.cs">
                                        <Input
                                            type="text"
                                            borderColor={border}
                                            bg={bg}
                                            placeholder={`${t('createProject.fields.genere').toString()} ${t(
                                                'createProject.language.cs',
                                            )}`}
                                        />
                                    </FormControl>

                                    <FormControl id="name.en">
                                        <Input
                                            type="text"
                                            borderColor={border}
                                            bg={bg}
                                            placeholder={`${t('createProject.fields.genere').toString()} ${t(
                                                'createProject.language.en',
                                            )}`}
                                        />
                                    </FormControl>
                                </Stack>
                            </FormControl>

                            <FormControl>
                                <FormLabel>{t('createProject.fields.description').toString()}</FormLabel>
                                <Stack direction={{ base: 'column', md: 'row' }}>
                                    <FormControl id="description.cs">
                                        <TextareaLimited
                                            placeholder={`${t('createProject.fields.description').toString()} ${t(
                                                'createProject.language.cs',
                                            )}`}
                                            limit={250}
                                            borderColor={border}
                                            bg={bg}
                                        />
                                    </FormControl>

                                    <FormControl id="description.en">
                                        <TextareaLimited
                                            placeholder={`${t('createProject.fields.description').toString()} ${t(
                                                'createProject.language.en',
                                            )}`}
                                            limit={250}
                                            borderColor={border}
                                            bg={bg}
                                        />
                                    </FormControl>
                                </Stack>
                            </FormControl>

                            <FormControl>
                                <FormLabel>{t('createProject.fields.movie').toString()}</FormLabel>
                                <Stack
                                    direction="column"
                                    borderColor={border}
                                    bg={bg}
                                    borderWidth={1}
                                    borderRadius="md"
                                    p={4}
                                    spacing={12}
                                >
                                    <Box>
                                        <FormLabel>{t('createProject.upload.filetype.video').toString()}</FormLabel>
                                        <Box
                                            w="100%"
                                            borderColor={border}
                                            bg={bg}
                                            borderWidth={1}
                                            borderRadius="md"
                                            py={4}
                                            px={6}
                                        >
                                            <Upload
                                                title={`${t('createProject.upload.filetype.video').toString()} ${t(
                                                    'generic.for',
                                                ).toString()} ${t(
                                                    'createProject.fields.movie',
                                                ).toString()}`.toLowerCase()}
                                                projectId={tempProjectId}
                                            />
                                            <Accordion allowToggle mx={-6} my={-4}>
                                                <AccordionItem borderWidth="0 !important">
                                                    <AccordionButton>
                                                        <Box as="span" flex="1" textAlign="left">
                                                            <Text fontWeight="bold" color={'gray.500'}>
                                                                {t('createProject.upload.specs').toString()}
                                                            </Text>
                                                        </Box>
                                                        <AccordionIcon />
                                                    </AccordionButton>
                                                    <AccordionPanel pb={4}>
                                                        <UnorderedList listStyleType="none" ml={0} color={'gray.500'}>
                                                            <ListItem>
                                                                Formát (kodek) videa: H.264 (doporučený), MPEG-4 Part 2
                                                            </ListItem>
                                                            <ListItem>
                                                                Formát (kodek) audia: WAV, FLAC, MP3 (bitrate u MP3
                                                                alespoň 192 kbps)
                                                            </ListItem>
                                                            <ListItem>Frame rate videa: 24 fps</ListItem>
                                                            <ListItem>
                                                                Titulky: anglické ve formátu SRT nebo ASS
                                                            </ListItem>
                                                            <ListItem>Kontejner: MP4 (doporučené), M4V, MKV</ListItem>
                                                            <ListItem>
                                                                Rozlišení: na šířku alespoň FullHD (t.j. 1920)
                                                            </ListItem>
                                                            <ListItem>Bitrate: 10 - 20 Mbps</ListItem>
                                                            <ListItem>Hlasitost: max. -3dB</ListItem>
                                                            <ListItem>Velikost: max. 2GB</ListItem>
                                                        </UnorderedList>
                                                    </AccordionPanel>
                                                </AccordionItem>
                                            </Accordion>
                                        </Box>
                                    </Box>
                                    <Box>
                                        <FormLabel>{t('createProject.upload.filetype.subtitles').toString()}</FormLabel>
                                        <Box
                                            w="100%"
                                            borderColor={border}
                                            bg={bg}
                                            borderWidth={1}
                                            borderRadius="md"
                                            py={4}
                                            px={6}
                                        >
                                            <Upload
                                                title={`${t('createProject.upload.filetype.subtitles').toString()} ${t(
                                                    'generic.for',
                                                ).toString()} ${t(
                                                    'createProject.fields.movie',
                                                ).toString()}`.toLowerCase()}
                                                projectId={tempProjectId}
                                            />
                                            <Accordion allowToggle mx={-6} my={-4}>
                                                <AccordionItem borderWidth="0 !important">
                                                    <AccordionButton>
                                                        <Box as="span" flex="1" textAlign="left">
                                                            <Text fontWeight="bold" color={'gray.500'}>
                                                                {t('createProject.upload.specs').toString()}
                                                            </Text>
                                                        </Box>
                                                        <AccordionIcon />
                                                    </AccordionButton>
                                                    <AccordionPanel pb={4}>
                                                        <UnorderedList listStyleType="none" ml={0} color={'gray.500'}>
                                                            <ListItem>
                                                                Formát (kodek) videa: H.264 (doporučený), MPEG-4 Part 2
                                                            </ListItem>
                                                            <ListItem>
                                                                Formát (kodek) audia: WAV, FLAC, MP3 (bitrate u MP3
                                                                alespoň 192 kbps)
                                                            </ListItem>
                                                            <ListItem>Frame rate videa: 24 fps</ListItem>
                                                            <ListItem>
                                                                Titulky: anglické ve formátu SRT nebo ASS
                                                            </ListItem>
                                                            <ListItem>Kontejner: MP4 (doporučené), M4V, MKV</ListItem>
                                                            <ListItem>
                                                                Rozlišení: na šířku alespoň FullHD (t.j. 1920)
                                                            </ListItem>
                                                            <ListItem>Bitrate: 10 - 20 Mbps</ListItem>
                                                            <ListItem>Hlasitost: max. -3dB</ListItem>
                                                            <ListItem>Velikost: max. 2GB</ListItem>
                                                        </UnorderedList>
                                                    </AccordionPanel>
                                                </AccordionItem>
                                            </Accordion>
                                        </Box>
                                    </Box>
                                </Stack>
                            </FormControl>

                            <FormControl>
                                <FormLabel>{t('createProject.fields.videoannotation').toString()}</FormLabel>
                                <Stack
                                    direction="column"
                                    borderColor={border}
                                    bg={bg}
                                    borderWidth={1}
                                    borderRadius="md"
                                    p={4}
                                    spacing={12}
                                >
                                    <Box>
                                        <FormLabel>{t('createProject.upload.filetype.video').toString()}</FormLabel>
                                        <Box
                                            w="100%"
                                            borderColor={border}
                                            bg={bg}
                                            borderWidth={1}
                                            borderRadius="md"
                                            py={4}
                                            px={6}
                                        >
                                            <Upload
                                                title={`${t('createProject.upload.filetype.video').toString()} ${t(
                                                    'generic.for',
                                                ).toString()} ${t(
                                                    'createProject.fields.videoannotation',
                                                ).toString()}`.toLowerCase()}
                                                projectId={tempProjectId}
                                            />
                                            <Accordion allowToggle mx={-6} my={-4}>
                                                <AccordionItem borderWidth="0 !important">
                                                    <AccordionButton>
                                                        <Box as="span" flex="1" textAlign="left">
                                                            <Text fontWeight="bold" color={'gray.500'}>
                                                                {t('createProject.upload.specs').toString()}
                                                            </Text>
                                                        </Box>
                                                        <AccordionIcon />
                                                    </AccordionButton>
                                                    <AccordionPanel pb={4}>
                                                        <UnorderedList listStyleType="none" ml={0} color={'gray.500'}>
                                                            <ListItem>
                                                                Formát (kodek) videa: H.264 (doporučený), MPEG-4 Part 2
                                                            </ListItem>
                                                            <ListItem>
                                                                Formát (kodek) audia: WAV, FLAC, MP3 (bitrate u MP3
                                                                alespoň 192 kbps)
                                                            </ListItem>
                                                            <ListItem>Frame rate videa: 24 fps</ListItem>
                                                            <ListItem>
                                                                Titulky: anglické ve formátu SRT nebo ASS
                                                            </ListItem>
                                                            <ListItem>Kontejner: MP4 (doporučené), M4V, MKV</ListItem>
                                                            <ListItem>
                                                                Rozlišení: na šířku alespoň FullHD (t.j. 1920)
                                                            </ListItem>
                                                            <ListItem>Bitrate: 10 - 20 Mbps</ListItem>
                                                            <ListItem>Hlasitost: max. -3dB</ListItem>
                                                            <ListItem>Velikost: max. 2GB</ListItem>
                                                        </UnorderedList>
                                                    </AccordionPanel>
                                                </AccordionItem>
                                            </Accordion>
                                        </Box>
                                    </Box>
                                    <Box>
                                        <FormLabel>{t('createProject.upload.filetype.subtitles').toString()}</FormLabel>
                                        <Box
                                            w="100%"
                                            borderColor={border}
                                            bg={bg}
                                            borderWidth={1}
                                            borderRadius="md"
                                            py={4}
                                            px={6}
                                        >
                                            <Upload
                                                title={`${t('createProject.upload.filetype.subtitles').toString()} ${t(
                                                    'generic.for',
                                                ).toString()} ${t(
                                                    'createProject.fields.videoannotation',
                                                ).toString()}`.toLowerCase()}
                                                projectId={tempProjectId}
                                            />
                                            <Accordion allowToggle mx={-6} my={-4}>
                                                <AccordionItem borderWidth="0 !important">
                                                    <AccordionButton>
                                                        <Box as="span" flex="1" textAlign="left">
                                                            <Text fontWeight="bold" color={'gray.500'}>
                                                                {t('createProject.upload.specs').toString()}
                                                            </Text>
                                                        </Box>
                                                        <AccordionIcon />
                                                    </AccordionButton>
                                                    <AccordionPanel pb={4}>
                                                        <UnorderedList listStyleType="none" ml={0} color={'gray.500'}>
                                                            <ListItem>
                                                                Formát (kodek) videa: H.264 (doporučený), MPEG-4 Part 2
                                                            </ListItem>
                                                            <ListItem>
                                                                Formát (kodek) audia: WAV, FLAC, MP3 (bitrate u MP3
                                                                alespoň 192 kbps)
                                                            </ListItem>
                                                            <ListItem>Frame rate videa: 24 fps</ListItem>
                                                            <ListItem>
                                                                Titulky: anglické ve formátu SRT nebo ASS
                                                            </ListItem>
                                                            <ListItem>Kontejner: MP4 (doporučené), M4V, MKV</ListItem>
                                                            <ListItem>
                                                                Rozlišení: na šířku alespoň FullHD (t.j. 1920)
                                                            </ListItem>
                                                            <ListItem>Bitrate: 10 - 20 Mbps</ListItem>
                                                            <ListItem>Hlasitost: max. -3dB</ListItem>
                                                            <ListItem>Velikost: max. 2GB</ListItem>
                                                        </UnorderedList>
                                                    </AccordionPanel>
                                                </AccordionItem>
                                            </Accordion>
                                        </Box>
                                    </Box>
                                </Stack>
                            </FormControl>

                            <Box h={64}></Box>

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
                    </Stack>
                )}
            </AwaitAPI>
        </OutletOrChildren>
    );
}
