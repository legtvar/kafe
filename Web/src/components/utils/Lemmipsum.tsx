import React, { useState } from 'react';

const texts = [
    'Laboratoř se specializuje na technologie zpracování a publikování rozsáhlých textových a multimediálních kolekcí dat včetně produkce a postprodukce filmů. Vyvíjí nové technologie založené zejména (ale nejen) na svobodném software.',
    'Laboratoř poskytuje zázemí výuce předmětů PV110 Základy filmové řeči, PV113 Produkce audiovizuálního díla, PV174 Seminář laboratoře LEMMA a PV114 Praktikum produkce kolektivního audiovizuálního díla, PB029 Elektronická příprava dokumentů.',
    'LEMMA spolupracuje s dalšími laboratořemi FI, zejména Laboratoří zpracování přirozeného jazyka (textová část publikačních projektů), Laboratoří pokročilých síťových technologií (střih a přenos videa), Laboratoří řeči a dialogu (dabing, zpracování zvuku), Laboratoří interakce člověka s počítačem (animace) a AGDaM – Ateliérem grafického designu a multimédií (grafický design).',
    'Z prostorových důvodů nouzově poskytuje přístřeší i výzkumné skupině MIR vedené stejným vedoucím, a dále studentům, jejichž bakalářské, diplomové a dizertační práce vede.',
    'Laboratoř se začala formovat v roce 1999 při práci na projektu CD Všech 5 pohromadě k pátému výročí FI. V roce 2001 pak finanční pomoc grantu Fondu rozvoje vysokých škol 433/2001 umožnila vybudovat střihové pracoviště, čímž se institucionali­zovala dosavadní činnost zakladatelů laboratoře. Téhož roku byla založena tradice studentských filmových festivalů, postavená výlučně na původních filmových dílech studentů MU. V roce 2004 v laboratoři vzniklo multimediální DVD 10@FI k desátému výročí založení FI. V letech 2006 a 2013 další granty Fondu rozvoje umožnily inovovat foto-video, počítačovou i prezentační techniku laboratoře a FI a umožnily další chod laboratoře.',
    'V laboratoři LEMMA se zpracovává celá řada projektů, od tradičního filmového festivalu, přes dokumentární filmy, až po reportážní snímky ve spolupráci s univerzitními organizacemi jakou jsou MUNIE, Studentská Unie Fakulty informatiky, ProFIdivadlo apod.',
    'Filmový festival Fakulty informatiky Masarykovy univerzity je výsledkem úsilí studentů MU, kteří během dvou semestrů v předmětech PV110 Základy filmové řeči a PV113 Produkce audiovizuálního díla absolvují kompletní výuku praktické produkce filmu, často bez předchozích zkušeností. V malých týmech i jednotlivě prochází celým procesem přípravy krátkometrážního filmu od námětů přes literární a technické scénáře, produkci, střih, postprodukci a zvládají práci scénáristů, režisérů, kameramanů, zvukařů, produkčních, střihačů, herců a dalších.',
    'Hotové snímky různých žánrů i technických přístupů jsou veřejně promítány na květnovém festivalu na půdě FI MU a od 16. ročníku i v Univerzitním kině Scala před téměř 1000 diváky. Nejlepší snímky jsou oceněny odbornou porotou a hlasováním diváků v sálech. Vítězové si odnesou peněžné odměny, věcné ceny od sponzorů a získají i putovní trofej – sošku Filmobola.',
    'První filmový festival vznikl v roce 2001 z nadšení z nových možností digitálního zpracování videa, z potřeb identifikovaných při přípravě CD Všech pět pohromadě a z touhy studentů FI vytvořit si svůj vlastní film, vyjádřit se jak slovem, tak obrazem. Během jednoho semestru vzniklo deset filmových etud, od počáteční autorovy představy, přes scénář po produkci, digitální střih a přípravu festivalu.',
    'Úspěch byl nevídaný, jak vystihla redaktorka studentského časopisu Informagika: „Fakt, že pod rukama informatiků vzniklo deset vtipných snímků, mi připadá podobně neuvěřitelný jako možnost, že deset angličtinářů napíše operační systém.“ Festival se od té doby opakoval již každoročně a laťka kvality i návštěvnost festivalu se neustále zvyšuje. V 16. ročníku dokonce z kapacitních důvodů expandoval a kromě prostor FI MU se promítá i v Univerzitním kině Scala.',
    'Laboratoř vyvíjí aplikace a technologie pro zpracování rozsáhlých kolekcí dat (například dokumentů digitálních knihoven, pro elektronické publikování), včetně multimediálních (produkce a postprodukce filmů). LEMMA je hřiště, kde vzniká otevřená informatika momentálně ve třech oblastech aplikovaného výzkumu, vědy a inovací.',
    'Na konci tohoto kurzu bude studentka/student schopna/schopen: Využít znalosti základů scenáristiky, dramaturgie, režie, produkce při psaní vlastního námětu, literárního a technického scénáře. Připravit technický scénář pro natáčení v navazujícím jarním semestru v kurzu PV113 Produkce audiovizuálního díla. Získat a prakticky využít znalosti z oblastí střihové skladby, záběrování, obsluhy kamerové techniky, svícení scény, zvučení scény a postprodukce. Vytvořit ze skupiny studentů filmový štáb s pevně definovanými rolemi a produkovat své vlastní krátké audiovizuální dílo.',
    'Chcete se vyjádřit filmovou řečí a zprodukovat svůj vlastní krátkometrážní film a pochlubit se jím před tisícovkou diváků na tradičním Filmovém festivalu Fakulty informatiky Masarykovy univerzity (FFFIMU)? Super, <3, pak pokračujte a vyberte si svou parketu v tradičním týmovém a zážitkovém projektu FFFIMU!',
    'Ochota pracovat na projektech laboratoře LEMMA (produkce tradičního filmového festivalu, využití videotechniky pro e-learning a příprava výukových videomateriálů, podpora výuky PV110 Základy filmové řeči a PV113 Produkce audiovizuálního díla, ...)',
];

export const Lemmipsum: React.FC = () => {
    const [id] = useState(Math.floor(Math.random() * texts.length));

    return <>{texts[id]}</>;
};

export const lemmipsum = () => {
    return texts[Math.floor(Math.random() * texts.length)];
};
