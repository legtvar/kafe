using System.Collections.Immutable;

namespace Kafe.Ruv;

public enum RegisterArtworkEnum
{
    Architektura_Architektura = 14,
    Architektura_UrbanismusAÚzemníPlánování = 15,
    Architektura_KrajinářskáArchitektura = 16,
    Architektura_VýstupyZadanéPředRokem2020 = 17,
    Audiovize = 3,
    Design_GrafickýDesign = 9,
    Design_MódaTextilŠperk = 11,
    Design_ProduktovýAPrůmyslovýDesign = 8,
    Design_SkloPorcelánKeramika = 10,
    Hudba = 4,
    Literatura = 5,
    ScénickáUmění = 6,
    VýtvarnáUmění_Restaurování = 13,
    VýtvarnáUmění_VýtvarnáUmění = 12,
}

public enum RegisterArtworkArtworkType
{
    Architektura = 2233,
    Dramaturgie = 2246,
    HereckýVýkonVAudiovizuálnímDíle = 2245,
    HlavníProdukce = 2239,
    Kamera = 2241,
    Kostýmy = 2247,
    Masky = 2248,
    RealizovanýScénář = 2240,
    Režie = 2238,
    RyzeAutorskéDílo = 2237,
    SeriálováTvorbaRežie = 2269,
    StřihováSkladba = 2242,
    Výprava = 2244,
    ZvukováSkladba = 2243,
}

public enum RegisterArtworkImpact
{
    A = 9005,
    B = 9006,
    C = 9007,
    D = 9008
}

public enum RegisterArtworkScope
{
    
}

public record RegisterArtworkParams(
    int Year, // Rok prvního uvedení
    RegisterArtworkEnum Segment,
    RegisterArtworkArtworkType ArtworkType, // Druh činnosti
    RegisterArtworkImpact Impact, // Závažnost a význam
    RegisterArtworkScope Scope, // Velikost
    string NameCs, // Název v originále, limit 254
    string KeywordsCs, // Klíčová slova v češtině, limit 254, new-line separated
    string AnotationCs, // Anotace v češtině, limit 4000, minimum nevíme
    string NameEn, // Název v angličtině
    string KeywordsEn, // Klíčovná slova v angličtině
    string AnotationEn, // Anotace v angličtině
    ImmutableArray<RegisterArtworkAuthor> Authors
);

public record RegisterArtworkAuthor(
    Guid Uuid,
    int Author,
    string FirstName,
    string LastName,
    string DegreeBeforeName,
    string DegreeAfterName,
    string Organization,
    int Share
);
