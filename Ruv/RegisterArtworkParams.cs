using System.Collections.Immutable;

namespace Kafe.Ruv;

/// <summary>
/// Original: Druh činnosti.
/// </summary>
public enum RegisterArtworkArtworkType
{
    /// <summary>
    /// Original: Architektura.
    /// </summary>
    Architecture = 2233,

    /// <summary>
    /// Original: Dramaturgie.
    /// </summary>
    Dramaturgy = 2246,

    /// <summary>
    /// Original: Herecký výkon v audiovizuálním díle.
    /// </summary>
    Actor = 2245,

    /// <summary>
    /// Original: Hlavní produkce.
    /// </summary>
    Production = 2239,

    /// <summary>
    /// Original: Kamera.
    /// </summary>
    Camera = 2241,

    /// <summary>
    /// Original: Kostýmy.
    /// </summary>
    Costumes = 2247,

    /// <summary>
    /// Original: Masky.
    /// </summary>
    Masks = 2248,

    /// <summary>
    /// Original: Realizovaný scénář.
    /// </summary>
    Script = 2240,

    /// <summary>
    /// Original: Režie.
    /// </summary>
    Director = 2238,

    /// <summary>
    /// Original: Ryze autorské dílo.
    /// </summary>
    AuthorsWork = 2237,

    /// <summary>
    /// Original: Seriálová tvorba režie.
    /// </summary>
    SeriesDirector = 2269,

    /// <summary>
    /// Original: Střihová skladba.
    /// </summary>
    Editor = 2242,

    /// <summary>
    /// Original: Výprava.
    /// </summary>
    ArtDirector = 2244,

    /// <summary>
    /// Original: Zvuková skladba.
    /// </summary>
    Sound = 2243,
}

public record RegisterArtworkParams(
    int Year, // Rok prvního uvedení
    int Segment,
    RegisterArtworkArtworkType ArtworkType,
    int Impact, // Závažnost a význam
    int Scope, // Velikost
    string NameCS, // Název v originále, limit 254
    ImmutableArray<string> KeywordsCS, // Klíčová slova v češtině, limit 254, new-line separated
    string AnnotationCS, // Anotace v češtině, limit 4000, minimum nevíme
    string NameEN, // Název v angličtině
    ImmutableArray<string> KeywordsEN, // Klíčovná slova v angličtině
    string AnnotationEN, // Anotace v angličtině
    ImmutableArray<RegisterArtworkAuthor> Authors,
    DateTimeOffset FestivalDate,
    ImmutableArray<Guid> Attachments,
    string? CitationLink = null, // link to the video
    string StudyProgram = RuvConst.DefaultStudyProgram,
    string StudySubject = RuvConst.DefaultStudySubject
);

public record RegisterArtworkAuthor(
    int Author,
    string FirstName,
    string LastName,
    string DegreeBeforeName,
    string DegreeAfterName,
    string Organization = ""
);
