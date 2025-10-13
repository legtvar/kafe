using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public partial class ProjectService
{
    public const int NameMaxLength = 42;
    public const int DescriptionMinLength = 50;
    public const int DescriptionMaxLength = 10000;
    public const int GenreMaxLength = 32;
    public static readonly TimeSpan FilmMinLength = TimeSpan.FromSeconds(1);
    public static readonly TimeSpan FilmMaxLength = TimeSpan.FromMinutes(8);
    public static readonly TimeSpan VideoAnnotationMinLength = TimeSpan.FromSeconds(1);
    public static readonly TimeSpan VideoAnnotationMaxLength = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan AcceptableDurationError = TimeSpan.FromSeconds(1);
    public const long FilmMaxFileLength = 2L << 30; // 2 GiB
    public const string FilmMaxFileLengthDescription = "2 GiB";
    public const long VideoMinBitrate = 10_000_000;
    public const string VideoMinBitrateDescription = "10 Mbps";
    public const long VideoMaxBitrate = 20_000_000;
    public const string VideoMaxBitrateDescription = "20 Mbps";
    public const long VideoBitrateTolerance = 1_000_000;
    public const int VideoShorterSideResolution = 1080;
    public const int CoverPhotoShorterSideResolution = 1080;
    public const double CoverPhotoMaxRatio = 16 / 9.0;
    public const string CoverPhotoMaxRatioDescription = "16:9";
    public const double CoverPhotoMinRatio = 4 / 3.0;
    public const string CoverPhotoMinRatioDescription = "4:3";
    public static readonly string[] AllowedContainers = new[]
    {
        "video/mp4",
        "video/x-matroska"
    };
    public static readonly string[] AllowedVideoCodecs = new[]
    {
        "h264",
        "mpeg4"
    };
    public static readonly string[] AllowedAudioCodecs = new[]
    {
        "aac",
        "mp3",
        "flac"
    };
    public static readonly string[] AllowedSubtitleCodecs = new[]
    {
        "subrip"
    };
    public static readonly string[] AllowedImageMimeTypes = new[]
    {
        "image/jpeg",
        "image/png"
    };
    public const long Mp3MinBitrate = 192000;
    public const string Mp3MinBitrateDescription = "192 kbps";
    public static readonly double[] RequiredFramerate = [24.0, 25.0];
    public const string RequiredFramerateDescriptionEnglish = "24 or 25 fps";
    public const string RequiredFramerateDescriptionCzech = "24 nebo 25 fps";

    public static long VideoAnnotationMaxFileLength = 512 << 20; // 512 MiB
    public static string VideoAnnotationMaxFileLengthDescription = "512 GiB";

    public const string InfoStage = "info";
    public const string FileStage = "file";
    public const string TechReviewStage = "tech-review";
    public const string VisualReviewStage = "visual-review";
    public const string DramaturgyReviewStage = "dramaturgy-review";

    public static readonly Diagnostic NameTooLong = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: InfoStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The project name in the '{0}' language is too long. It may have at most {1} characters."),
            (Const.CzechCulture, "Název projektu v jazyce '{0}' je příliš dlouhý. Může mít nanajevýš {1} znaků.")
        )
    );

    public static readonly Diagnostic NameTooShort = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: InfoStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The project name in the '{0}' language is too short. It must have at least {1} characters."),
            (Const.CzechCulture, "Název projektu v jazyce '{0}' je příliš krátký. Musí míň alespoň {1} znaků.")
        )
    );

    public static readonly Diagnostic MissingName = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: InfoStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The project is missing a name."),
            (Const.CzechCulture, "Projektu chybí název.")
        )
    );

    public static readonly Diagnostic MissingNameCulture = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: InfoStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The project is missing a name in the '{0}' language."),
            (Const.CzechCulture, "Projektu chybí název v jazyce '{0}'.")
        )
    );

    public static readonly Diagnostic DescriptionTooLong = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: InfoStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The project description in the '{0}' language is too long. It may have at most {1} characters."),
            (Const.CzechCulture, "Popis projektu v jazyce '{0}' je příliš dlouhý. Může mít nanajevýš {1} znaků.")
        )
    );

    public static readonly Diagnostic DescriptionTooShort = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: InfoStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The project description in the '{0}' language is too short. It must have at least {1} characters."),
            (Const.CzechCulture, "Popis projektu v jazyce '{0}' je příliš krátký. Musí míň alespoň {1} znaků.")
        )
    );

    public static readonly Diagnostic MissingDescription = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: InfoStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The project is missing a description."),
            (Const.CzechCulture, "Projektu chybí popis.")
        )
    );

    public static readonly Diagnostic MissingDescriptionCulture = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: InfoStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The project is missing a description in the '{0}' language."),
            (Const.CzechCulture, "Projektu chybí popis v jazyce '{0}'.")
        )
    );

    public static readonly Diagnostic GenreTooLong = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: InfoStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The project genre in the '{0}' language is too long. It may have at most {1} characters."),
            (Const.CzechCulture, "Žánr projektu v jazyce '{0}' je příliš dlouhý. Může mít nanajevýš {1} znaků.")
        )
    );

    public static readonly Diagnostic GenreTooShort = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: InfoStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The project genre in the '{0}' language is too short. It must have at least {1} characters."),
            (Const.CzechCulture, "Žánr projektu v jazyce '{0}' je příliš krátký. Musí mít alespoň {1} znaků.")
        )
    );

    public static readonly Diagnostic MissingGenre = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: InfoStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The project is missing a genre."),
            (Const.CzechCulture, "Projektu chybí žánr.")
        )
    );

    public static readonly Diagnostic MissingGenreCulture = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: InfoStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The project is missing a genre in the '{0}' language."),
            (Const.CzechCulture, "Projektu chybí žánr v jazyce '{0}'.")
        )
    );

    public static readonly Diagnostic MissingTechReview = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: TechReviewStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "This project is yet to be accepted by a festival technician."),
            (Const.CzechCulture, "Tento project ještě musí přijmout festivalový technik.")
        )
    );

    public static readonly Diagnostic MissingVisualReview = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: VisualReviewStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "This project is yet to be accepted by a festival graphic designer."),
            (Const.CzechCulture, "Tento project ještě musí přijmout festivalový grafik.")
        )
    );

    public static readonly Diagnostic MissingDramaturgyReview = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: DramaturgyReviewStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "This project is yet to be accepted by a festival jury."),
            (Const.CzechCulture, "Tento project ještě musí přijmout festivalová porota.")
        )
    );

    public static readonly Diagnostic MissingCrew = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: InfoStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "At least one crew member is required."),
            (Const.CzechCulture, "Je vyžadován alespoň jeden člen štábu.")
        )
    );

    public static readonly Diagnostic MissingFilm = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The film is required."),
            (Const.CzechCulture, "Film je povinnou součástí přihlášky.")
        )
    );

    public static readonly Diagnostic TooManyFilms = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "Only one film is permitted."),
            (Const.CzechCulture, "Součástí přihlášky může být pouze jeden film.")
        )
    );

    public static readonly Diagnostic MissingVideoAnnotationVideo = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The video-annotation is missing the video file."),
            (Const.CzechCulture, "Videoanotaci chybí soubor s videem.")
        )
    );

    public static readonly Diagnostic TooManyVideoAnnotations = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "Only one video-annotation is permitted."),
            (Const.CzechCulture, "Součástí přihlášky může být pouze jedna videoanotace.")
        )
    );

    public static readonly Diagnostic MissingFilmSubtitles = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "Subtitles for the film file are required."),
            (Const.CzechCulture, "Titulky pro film jsou povinné.")
        )
    );

    public static readonly Diagnostic MissingVideoAnnotationSubtitles = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The video-annotation is missing subtitles."),
            (Const.CzechCulture, "Videoanotaci chybí soubor s titulky.")
        )
    );

    public static readonly Diagnostic TooManyFilmSubtitles = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "Only one file with subtitles for the film is permitted."),
            (Const.CzechCulture, "Pouze jedny titulky k filmu mohou být součástí přihlášky.")
        )
    );

    public static readonly Diagnostic TooManyVideoAnnotationSubtitles = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "Only one file with subtitles for the video-annotation is permitted."),
            (Const.CzechCulture, "Pouze jedny titulky k videoanotaci mohou být součástí přihlášky.")
        )
    );

    public static readonly Diagnostic TooFewCoverPhotos = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "At least one cover photo is required."),
            (Const.CzechCulture, "Alespoň jedna titulní fotka je vyžadována.")
        )
    );

    public static readonly Diagnostic TooManyCoverPhotos = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "Too many cover photos have been provided."),
            (Const.CzechCulture, "Přihláška obsahuje příliš mnoho titulních fotek.")
        )
    );

    public static readonly Diagnostic FilmTooShort = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The film is too short. Minimum length is '{FilmMinLength:c}'."),
            (Const.CzechCulture, $"Film je příliš krátký. Minimální délka je '{FilmMinLength:c}'.")
        )
    );

    public static readonly Diagnostic FilmTooLong = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The film is too long. Maximum length is '{FilmMaxLength:c}'."),
            (Const.CzechCulture, $"Film je příliš dlouhý. Maximální délka je '{FilmMaxLength:c}'.")
        )
    );

    public static readonly Diagnostic VideoAnnotationTooShort = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The video-annotation is too short. Minimum length is '{FilmMinLength:c}'."),
            (Const.CzechCulture, $"Videoanotace je příliš krátká. Minimální délka je '{FilmMinLength:c}'.")
        )
    );

    public static readonly Diagnostic VideoAnnotationTooLong = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The video-annotation is too long. Maximum length is '{FilmMaxLength:c}'."),
            (Const.CzechCulture, $"Videoanotace je příliš krátká. Maximální délka je '{FilmMaxLength:c}'.")
        )
    );

    public static readonly Diagnostic FilmHasZeroFileLength = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The film file has zero file length."),
            (Const.CzechCulture, "Soubor s filmem má nulovou velikost.")
        )
    );

    public static readonly Diagnostic FilmIsTooLarge = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The film file is larger than {FilmMaxFileLengthDescription}."),
            (Const.CzechCulture, $"Soubor s filmem je větší než {FilmMaxFileLengthDescription}.")
        )
    );

    public static readonly Diagnostic VideoAnnotationHasZeroFileLength = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The video-annotation file has zero file length."),
            (Const.CzechCulture, "Soubor s videoanotací má nulovou velikost.")
        )
    );

    public static readonly Diagnostic VideoAnnotationIsTooLarge = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The video-annotation file is larger than {VideoAnnotationMaxFileLengthDescription}."),
            (Const.CzechCulture, $"Soubor s videoanotací je větší než {VideoAnnotationMaxFileLengthDescription}.")
        )
    );

    public static readonly Diagnostic FilmCorrupted = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The film file is corrupted."),
            (Const.CzechCulture, "Soubor s filmem je poškozený.")
        )
    );

    public static readonly Diagnostic VideoAnnotationCorrupted = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The video-annotation file is corrupted."),
            (Const.CzechCulture, "Soubor s videoanotací je poškozený.")
        )
    );

    public static readonly Diagnostic CoverPhotoCorrupted = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "A cover photo file is corrupted."),
            (Const.CzechCulture, "Některá z titulních fotek je poškozená.")
        )
    );

    public static readonly Diagnostic SubtitlesCorrupted = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "A subtitles file is corrupted."),
            (Const.CzechCulture, "Některý ze souborů s titulky je poškozený.")
        )
    );

    public static readonly Diagnostic FilmUnsupportedContainerFormat = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The film file uses an unsupported format. " +
                "The supported formats are MP4, M4V, and MKV."),
            (Const.CzechCulture, "Soubor s filmem použivá nepodporovaný kontejner. " +
                "Podporovanými kontejnery jsou MP4, M4V a MKV.")
        )
    );

    public static readonly Diagnostic VideoAnnotationUnsupportedContainerFormat = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The video-annotation file uses an unsupported format. " +
                "The supported formats are MP4, M4V, and MKV."),
            (Const.CzechCulture, "Soubor s videoanotací použivá nepodporovaný kontejner. " +
                "Podporovanými kontejnery jsou MP4, M4V a MKV.")
        )
    );

    public static readonly Diagnostic FilmUnsupportedVideoCodec = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The film file uses an unsupported video codec. " +
                "The supported video codecs are H.264 and MPEG-4 Part 2."),
            (Const.CzechCulture, "Soubor s filmem použivá nepodporovaný video kodek. " +
                "Podporovanými video kodeky jsou H.264 a MPEG-4 Part 2.")
        )
    );

    public static readonly Diagnostic VideoAnnotationUnsupportedVideoCodec = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The video-annotation file uses an unsupported video codec. " +
                "The supported video codecs are H.264 and MPEG-4 Part 2."),
            (Const.CzechCulture, "Soubor s videoanotací použivá nepodporovaný video kodek. " +
                "Podporovanými video kodeky jsou H.264 a MPEG-4 Part 2.")
        )
    );

    public static readonly Diagnostic FilmUnsupportedAudioCodec = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The film file uses an unsupported audio codec. " +
                $"The supported audio codecs are AAC, FLAC, and MP3 with bitrate at least {Mp3MinBitrateDescription}."),
            (Const.CzechCulture, "Soubor s filmem použivá nepodporovaný audio kodek. " +
                $"Podporovanými audio kodeky jsou AAC, FLAC a MP3 s bitratem alespoň {Mp3MinBitrateDescription}.")
        )
    );

    public static readonly Diagnostic VideoAnnotationUnsupportedAudioCodec = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The video-annotation file uses an unsupported audio codec. " +
                $"The supported audio codecs are AAC, FLAC, and MP3 with bitrate at least {Mp3MinBitrateDescription}."),
            (Const.CzechCulture, "Soubor s videoanotací použivá nepodporovaný audio kodek. " +
                $"Podporovanými audio kodeky jsou AAC, FLAC a MP3 s bitratem alespoň {Mp3MinBitrateDescription}.")
        )
    );

    public static readonly Diagnostic FilmMp3BitrateTooLow = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The bitrate of the MP3 audio in the film file must be at least {Mp3MinBitrateDescription}."),
            (Const.CzechCulture, $"Bitrate MP3 audia v souboru s filmem musí být alespoň {Mp3MinBitrateDescription}.")
        )
    );

    public static readonly Diagnostic VideoAnnotationMp3BitrateTooLow = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The bitrate of the MP3 audio in the video-annotation file must be at least {Mp3MinBitrateDescription}."),
            (Const.CzechCulture, $"Bitrate MP3 audia v souboru s videoanotací musí být alespoň {Mp3MinBitrateDescription}.")
        )
    );

    public static readonly Diagnostic FilmUnsupportedFramerate = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The film file has an unsupported framerate. The required framerate is {RequiredFramerateDescriptionEnglish}."),
            (Const.CzechCulture, $"Soubor s filmem má nepodporovaný framerate. Vyžadovaný framerate je {RequiredFramerateDescriptionCzech}.")
        )
    );

    public static readonly Diagnostic VideoAnnotationUnsupportedFramerate = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The video-annotation file has an unsupported framerate. The required framerate is {RequiredFramerateDescriptionEnglish}."),
            (Const.CzechCulture, $"Soubor s videoanotací má nepodporovaný framerate. Vyžadovaný framerate je {RequiredFramerateDescriptionCzech}.")
        )
    );

    public static readonly Diagnostic FilmInvalidStreamCount = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The film file must have only one video stream and one audio stream."),
            (Const.CzechCulture, "Soubor s filmem musí mít pouze jeden video stream a jeden audio stream.")
        )
    );

    public static readonly Diagnostic VideoAnnotationInvalidStreamCount = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The video-annotation file must have only one video stream and one audio stream."),
            (Const.CzechCulture, "Soubor s videoanotací musí mít pouze jeden video stream a jeden audio stream.")
        )
    );

    public static readonly Diagnostic FilmWrongResolution = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The film file must have a shorter-side resolution of at least {VideoShorterSideResolution} pixels."),
            (Const.CzechCulture, $"Soubor s filmem musí mít na kratší straně rozlišení alespoň {VideoShorterSideResolution} pixelů.")
        )
    );

    public static readonly Diagnostic VideoAnnotationWrongResolution = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The video-annotation file must have a shorter-side resolution of at least {VideoShorterSideResolution} pixels."),
            (Const.CzechCulture, $"Soubor s videoanotací musí mít na kratší straně rozlišení alespoň {VideoShorterSideResolution} pixelů.")
        )
    );

    public static readonly Diagnostic CoverPhotoWrongResolution = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"All cover photo images must have a shorter-side resolution of at least {CoverPhotoShorterSideResolution}."),
            (Const.CzechCulture, $"Všechny titulní fotky musí mít na kratší straně rozlišení alespoň {CoverPhotoShorterSideResolution} pixelů.")
        )
    );

    public static readonly Diagnostic CoverPhotoWrongFormat = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"All cover photo images must be in the JPEG or PNG format."),
            (Const.CzechCulture, $"Všechny titulní fotky musí být buď ve formátu JPEG, nebo PNG.")
        )
    );

    public static readonly Diagnostic SubtitlesWrongFormat = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"All subtitles must be in the SRT or ASS format."),
            (Const.CzechCulture, $"Všechny titulky musí být buď ve formátu SRT, nebo ASS.")
        )
    );

    public static readonly Diagnostic FilmUnsupportedBitrate = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The film file must have a bitrate between {VideoMinBitrateDescription} and {VideoMaxBitrateDescription}."),
            (Const.CzechCulture, $"Soubor s filmem musí mít bitrate mezi {VideoMinBitrateDescription} a {VideoMaxBitrateDescription}.")
        )
    );

    public static readonly Diagnostic VideoAnnotationUnsupportedBitrate = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The video-annotation file must have a bitrate between {VideoMinBitrateDescription} and {VideoMaxBitrateDescription}."),
            (Const.CzechCulture, $"Soubor s videoanotací musí mít bitrate mezi {VideoMinBitrateDescription} a {VideoMaxBitrateDescription}.")
        )
    );

    public static readonly Diagnostic FilmSubtitlesUnsupportedCodec = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The film subtites have an unsupported format. The supported formats are SRT and ASS."),
            (Const.CzechCulture, $"Titulky k filmu mají nepodporovaný formát. Podporovanými formáty jsou SRT a ASS.")
        )
    );

    public static readonly Diagnostic VideoAnnotationSubtitlesUnsupportedCodec = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The video-annotation subtites have an unsupported format. The supported formats are SRT and ASS."),
            (Const.CzechCulture, $"Titulky k videoanotaci mají nepodporovaný formát. Podporovanými formáty jsou SRT a ASS.")
        )
    );

    public static readonly Diagnostic MissingCoverPhotoFile = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"A cover photo is missing the image file."),
            (Const.CzechCulture, $"Některé z titulních fotek chybí obrazový soubor.")
        )
    );

    public static readonly Diagnostic MissingSubtitlesFile = new Diagnostic(
    Kind: DiagnosticKind.Error,
    ValidationStage: FileStage,
    Message: LocalizedString.Create(
        (Const.InvariantCulture, $"Some subtitles are missing the subtitles file."),
        (Const.CzechCulture, $"Některým z titulků chybí soubor s titulky.")
    )
);

    public static readonly Diagnostic CoverPhotoWrongRatio = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"All cover photos must have aspect ratio between " +
                $"{CoverPhotoMinRatioDescription} and {CoverPhotoMaxRatioDescription}."),
            (Const.CzechCulture, $"Všechny titulní fotky musí mít poměr stran mezi " +
                $"{CoverPhotoMinRatioDescription} a {CoverPhotoMaxRatioDescription}.")
        )
    );
    public static readonly Diagnostic MissingPigeonsTestResult = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "PIGEOnS test result cannot be found."),
            (Const.CzechCulture, "Výsledek PIGEOnS testu nelze nalézt.")
        )
    );

    public static readonly Diagnostic IncorrectPigeonsTestResultFormat = new Diagnostic(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "PIGEOnS test result is of incorrect format."),
            (Const.CzechCulture, "Výsledek PIGEOnS testu má nesprávný formát.")
        )
    );

    public ProjectReport ValidateBasicInfo(ProjectInfo project, ProjectValidationSettings? settings = null)
    {
        settings = ProjectValidationSettings.Merge(settings, ProjectValidationSettings.Default);
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

        ValidateLocalizedString(
            diagnostics,
            project.Name,
            settings.MinNameLength,
            settings.MaxNameLength,
            settings.RequiredNameCultures,
            MissingName,
            MissingNameCulture,
            NameTooLong,
            NameTooShort
        );
        ValidateLocalizedString(
            diagnostics,
            project.Description,
            settings.MinDescriptionLength,
            settings.MaxDescriptionLength,
            settings.RequiredDescriptionCultures,
            MissingDescription,
            MissingDescriptionCulture,
            DescriptionTooLong,
            DescriptionTooShort
        );
        ValidateLocalizedString(
            diagnostics,
            project.Genre,
            settings.MinGenreLength,
            settings.MaxGenreLength,
            settings.RequiredGenreCultures,
            MissingGenre,
            MissingGenreCulture,
            GenreTooLong,
            GenreTooShort
        );

        return new ProjectReport(
            ProjectId: project.Id,
            ValidatedOn: DateTimeOffset.UtcNow,
            Diagnostics: diagnostics.ToImmutable()
        );
    }

    private static void ValidateLocalizedString(
        IList<Diagnostic> diagnostics,
        LocalizedString? ls,
        int? minLength,
        int? maxLength,
        ImmutableHashSet<string>? requiredCultures,
        Diagnostic missing,
        Diagnostic missingCulture,
        Diagnostic tooLong,
        Diagnostic tooShort
    )
    {
        if (minLength is > 0 && LocalizedString.IsNullOrEmpty(ls))
        {
            diagnostics.Add(missing);
        }

        if (requiredCultures is not null && !requiredCultures.IsEmpty)
        {
            foreach (var culture in requiredCultures)
            {
                if (ls is null || !ls.HasVariant(culture))
                {
                    diagnostics.Add(missingCulture.Format(Const.GetLanguageName(culture)));
                }
            }
        }

        if (!LocalizedString.IsNullOrEmpty(ls))
        {
            foreach (var (culture, variant) in ls)
            {
                if (minLength is not null && variant.Length < minLength)
                {
                    diagnostics.Add(tooShort.Format(Const.GetLanguageName(culture), minLength.Value));
                }

                if (maxLength is not null && variant.Length > maxLength)
                {
                    diagnostics.Add(tooLong.Format(Const.GetLanguageName(culture), maxLength.Value));
                }
            }
        }
    }

    // TODO: Blueprints instead of these hard-coded validation settings for FFFI MU 2023.
    public async Task<ProjectReport> Validate(Hrib id, CancellationToken token = default)
    {
        var project = await db.Events.RequireLatest<ProjectInfo>(id, token);
        if (project is null)
        {
            throw new ArgumentException($"Project '{id}' does not exist.");
        }

        var projectGroup = await db.Events.RequireLatest<ProjectGroupInfo>(project.ProjectGroupId, token);
        if (projectGroup is null)
        {
            throw new ArgumentException($"Project group '{project.ProjectGroupId}' does not exist.");
        }

        var settings = ProjectValidationSettings.Merge(
            projectGroup.ValidationSettings,
            ProjectValidationSettings.Default
        );

        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

        var basicReport = ValidateBasicInfo(project, settings);
        diagnostics.AddRange(basicReport.Diagnostics);

        if (!project.Reviews.Any(r => r.ReviewerRole == Const.TechReviewer && r.Kind == ReviewKind.Accepted))
        {
            diagnostics.Add(MissingTechReview);
        }

        if (!project.Reviews.Any(r => r.ReviewerRole == Const.VisualReviewer && r.Kind == ReviewKind.Accepted))
        {
            diagnostics.Add(MissingVisualReview);
        }

        if (!project.Reviews.Any(r => r.ReviewerRole == Const.VisualReviewer && r.Kind == ReviewKind.Accepted))
        {
            diagnostics.Add(MissingDramaturgyReview);
        }

        if (project.Authors.Count(a => a.Kind == ProjectAuthorKind.Crew) < 1)
        {
            diagnostics.Add(MissingCrew);
        }

        var artifactInfos = await db.LoadManyAsync<ArtifactDetail>(token, project.Artifacts.Select(a => a.Id));
        var artifacts = project.Artifacts
            .Join(artifactInfos, a => a.Id, i => i.Id, (a, i) => (projectArtifact: a, info: i))
            .ToImmutableArray();

        var filmArtifacts = artifacts.Where(a => a.projectArtifact.BlueprintSlot == Const.FilmBlueprintSlot)
            .ToImmutableArray();
        if (filmArtifacts.Length == 0)
        {
            diagnostics.Add(MissingFilm);
        }
        else if (filmArtifacts.Length > 1)
        {
            diagnostics.Add(TooManyFilms);
        }
        else
        {
            var videoShards = filmArtifacts.Single().info.Shards.Where(s => s.Kind == ShardKind.Video)
                .ToImmutableArray();
            if (videoShards.Length == 0)
            {
                diagnostics.Add(MissingFilm);
            }
            else if (videoShards.Length > 1)
            {
                diagnostics.Add(TooManyFilms);
            }
            else
            {
                var videoShard = await db.LoadAsync<VideoShardInfo>(videoShards[0].ShardId, token);
                if (videoShard is null)
                {
                    diagnostics.Add(MissingFilm);
                }
                else
                {
                    diagnostics.AddRange(ValidateVideo(
                        video: videoShard,
                        maxFileLength: FilmMaxFileLength,
                        minLength: FilmMinLength,
                        maxLength: FilmMaxLength,
                        corruptedError: FilmCorrupted,
                        zeroFileLengthError: FilmHasZeroFileLength,
                        tooLargeError: FilmIsTooLarge,
                        tooShortError: FilmTooShort,
                        tooLongError: FilmTooLong,
                        streamMismatchError: FilmInvalidStreamCount,
                        unsupportedContainerError: FilmUnsupportedContainerFormat,
                        bitrateTooLowError: FilmUnsupportedBitrate,
                        bitrateTooHighError: FilmUnsupportedBitrate,
                        unsupportedVideoCodecError: FilmUnsupportedVideoCodec,
                        unsupportedAudioCodecError: FilmUnsupportedAudioCodec,
                        mp3BitrateTooLowError: FilmMp3BitrateTooLow,
                        unsupportedFramerateError: FilmUnsupportedFramerate,
                        wrongResolutionError: FilmWrongResolution
                    ));
                }
            }

            var subtitleShards = filmArtifacts.Single().info.Shards.Where(s => s.Kind == ShardKind.Subtitles)
                .ToImmutableArray();
            if (subtitleShards.Length == 0)
            {
                diagnostics.Add(MissingFilmSubtitles);
            }
            else if (subtitleShards.Length > 1)
            {
                diagnostics.Add(TooManyFilmSubtitles);
            }
            else
            {
                var subtitleShard = await db.LoadAsync<SubtitlesShardInfo>(subtitleShards[0].ShardId, token);
                if (subtitleShard is null)
                {
                    diagnostics.Add(MissingFilmSubtitles);
                }
                else
                {
                    diagnostics.AddRange(ValidateSubtitles(subtitleShard));
                }
            }
        }

        var videoAnnotationArtifacts = artifacts.Where(a => a.projectArtifact.BlueprintSlot == Const.VideoAnnotationBlueprintSlot)
            .ToImmutableArray();
        if (videoAnnotationArtifacts.Length > 1)
        {
            diagnostics.Add(TooManyVideoAnnotations);
        }
        else if (videoAnnotationArtifacts.Length == 1)
        {
            var videoShards = videoAnnotationArtifacts.Single().info.Shards.Where(s => s.Kind == ShardKind.Video)
                .ToImmutableArray();
            if (videoShards.Length == 0)
            {
                diagnostics.Add(MissingVideoAnnotationVideo);
            }
            else if (videoShards.Length > 1)
            {
                diagnostics.Add(TooManyVideoAnnotations);
            }
            else
            {
                var videoShard = await db.LoadAsync<VideoShardInfo>(videoShards[0].ShardId, token);
                if (videoShard is null)
                {
                    diagnostics.Add(MissingFilm);
                }
                else
                {
                    diagnostics.AddRange(ValidateVideo(
                        video: videoShard,
                        maxFileLength: VideoAnnotationMaxFileLength,
                        minLength: VideoAnnotationMinLength,
                        maxLength: VideoAnnotationMaxLength,
                        corruptedError: VideoAnnotationCorrupted,
                        zeroFileLengthError: VideoAnnotationHasZeroFileLength,
                        tooLargeError: VideoAnnotationIsTooLarge,
                        tooShortError: VideoAnnotationTooShort,
                        tooLongError: VideoAnnotationTooLong,
                        streamMismatchError: VideoAnnotationInvalidStreamCount,
                        unsupportedContainerError: VideoAnnotationUnsupportedContainerFormat,
                        bitrateTooLowError: VideoAnnotationUnsupportedBitrate,
                        bitrateTooHighError: VideoAnnotationUnsupportedBitrate,
                        unsupportedVideoCodecError: VideoAnnotationUnsupportedVideoCodec,
                        unsupportedAudioCodecError: VideoAnnotationUnsupportedAudioCodec,
                        mp3BitrateTooLowError: VideoAnnotationMp3BitrateTooLow,
                        unsupportedFramerateError: VideoAnnotationUnsupportedFramerate,
                        wrongResolutionError: VideoAnnotationWrongResolution
                    ));
                }
            }

            var subtitleShards = videoAnnotationArtifacts.Single().info.Shards.Where(s => s.Kind == ShardKind.Subtitles)
                .ToImmutableArray();
            if (subtitleShards.Length == 0)
            {
                diagnostics.Add(MissingVideoAnnotationSubtitles);
            }
            else if (subtitleShards.Length > 1)
            {
                diagnostics.Add(TooManyVideoAnnotationSubtitles);
            }
            else
            {
                var subtitleShard = await db.LoadAsync<SubtitlesShardInfo>(subtitleShards[0].ShardId, token);
                if (subtitleShard is null)
                {
                    diagnostics.Add(MissingVideoAnnotationSubtitles);
                }
                else
                {
                    diagnostics.AddRange(ValidateSubtitles(subtitleShard));
                }
            }
        }

        var coverPhotoArtifacts = artifacts.Where(a => a.projectArtifact.BlueprintSlot == Const.CoverPhotoBlueprintSlot)
            .ToImmutableArray();
        if (coverPhotoArtifacts.Length < Const.CoverPhotoMinCount)
        {
            diagnostics.Add(TooFewCoverPhotos);
        }
        else if (coverPhotoArtifacts.Length > Const.CoverPhotoMaxCount)
        {
            diagnostics.Add(TooManyCoverPhotos);
        }
        else
        {
            foreach (var coverPhotoArtifact in coverPhotoArtifacts)
            {
                if (coverPhotoArtifact.info.Shards.Length != 1)
                {
                    diagnostics.Add(MissingCoverPhotoFile);
                    continue;
                }

                var imageShard = await db.LoadAsync<ImageShardInfo>(
                    coverPhotoArtifact.info.Shards.Single().ShardId,
                    token);
                if (imageShard is null)
                {
                    diagnostics.Add(MissingCoverPhotoFile);
                    continue;
                }
                diagnostics.AddRange(ValidateImage(imageShard));
            }
        }

        var blendArtifacts = artifacts.Where(a => a.projectArtifact.BlueprintSlot == Const.BlendBlueprintSlot);
        var blendShards = blendArtifacts.SelectMany(a => a.info.Shards.Where(s => s.Kind == ShardKind.Blend))
            .ToImmutableArray();
        foreach (var blendShard in blendShards)
        {
            var blendShardInfo = await db.LoadAsync<BlendShardInfo>(blendShard.ShardId, token);
            diagnostics.AddRange(ValidatePigeons(blendShardInfo));
        }

        return new(
            ProjectId: id,
            ValidatedOn: DateTimeOffset.UtcNow,
            Diagnostics: diagnostics.ToImmutable()
        );
    }

    public async Task<Err<bool>> AddReview(
        Hrib projectId,
        Hrib? reviewerId,
        ReviewKind kind,
        string reviewerRole,
        LocalizedString? comment,
        CancellationToken token = default)
    {
        var project = await Load(projectId, token);
        if (project is null)
        {
            return new Error($"Project {projectId} could not be found.");
        }

        if (kind == ReviewKind.NotReviewed)
        {
            return new Error($"Review cannot be '{kind}'. It must be either accepting or rejecting.");
        }

        var eventStream = await db.Events.FetchForExclusiveWriting<ProjectInfo>(projectId.ToString(), token);
        var reviewAdded = new ProjectReviewAdded(projectId.ToString(), reviewerId?.ToString(), kind, reviewerRole, comment);
        eventStream.AppendOne(reviewAdded);
        await db.SaveChangesAsync(token);
        return true;
    }

    private static IEnumerable<Diagnostic> ValidateVideo(
        VideoShardInfo video,
        long maxFileLength,
        TimeSpan minLength,
        TimeSpan maxLength,
        Diagnostic corruptedError,
        Diagnostic zeroFileLengthError,
        Diagnostic tooLargeError,
        Diagnostic tooShortError,
        Diagnostic tooLongError,
        Diagnostic streamMismatchError,
        Diagnostic unsupportedContainerError,
        Diagnostic bitrateTooLowError,
        Diagnostic bitrateTooHighError,
        Diagnostic unsupportedVideoCodecError,
        Diagnostic unsupportedAudioCodecError,
        Diagnostic mp3BitrateTooLowError,
        Diagnostic unsupportedFramerateError,
        Diagnostic wrongResolutionError)
    {
        if (!video.Variants.ContainsKey(Const.OriginalShardVariant))
        {
            yield return MissingFilm;
            yield break;
        }

        var originalVariant = video.Variants[Const.OriginalShardVariant];
        if (originalVariant.IsCorrupted)
        {
            yield return corruptedError;
            yield break;
        }

        if (originalVariant.FileLength == 0)
        {
            yield return zeroFileLengthError;
        }
        else if (originalVariant.FileLength > maxFileLength)
        {
            yield return tooLargeError;
        }

        if (originalVariant.Duration + AcceptableDurationError < minLength)
        {
            yield return tooShortError;
        }
        else if (originalVariant.Duration - AcceptableDurationError > maxLength)
        {
            yield return tooLongError;
        }

        var mimeType = originalVariant.MimeType;
        if (mimeType is null || !AllowedContainers.Contains(mimeType))
        {
            yield return unsupportedContainerError;
        }

        if (originalVariant.VideoStreams.Length != 1
            || originalVariant.AudioStreams.Length != 1
            || originalVariant.SubtitleStreams.Length != 0)
        {
            yield return streamMismatchError;
            yield break;
        }

        if (originalVariant.Bitrate < VideoMinBitrate - VideoBitrateTolerance)
        {
            yield return bitrateTooLowError;
        }
        else if (originalVariant.Bitrate > VideoMaxBitrate + VideoBitrateTolerance)
        {
            yield return bitrateTooHighError;
        }

        var videoStream = originalVariant.VideoStreams.Single();
        var audioStream = originalVariant.AudioStreams.Single();


        if (!AllowedVideoCodecs.Contains(videoStream.Codec))
        {
            yield return unsupportedVideoCodecError;
        }

        if (!AllowedAudioCodecs.Contains(audioStream.Codec))
        {
            yield return unsupportedAudioCodecError;
        }

        if (audioStream.Codec == "mp3" && audioStream.Bitrate < Mp3MinBitrate)
        {
            yield return mp3BitrateTooLowError;
        }

        if (!RequiredFramerate.Contains(videoStream.Framerate))
        {
            yield return unsupportedFramerateError;
        }

        var shorterSideResolution = Math.Min(videoStream.Width, videoStream.Height);
        if (shorterSideResolution < VideoShorterSideResolution)
        {
            yield return wrongResolutionError;
        }
    }

    private static IEnumerable<Diagnostic> ValidateImage(ImageShardInfo image)
    {
        if (!image.Variants.ContainsKey(Const.OriginalShardVariant))
        {
            yield return MissingCoverPhotoFile;
        }

        var originalVariant = image.Variants[Const.OriginalShardVariant];

        if (originalVariant.IsCorrupted)
        {
            yield return CoverPhotoCorrupted;
        }

        var shorterSideResolution = Math.Min(originalVariant.Width, originalVariant.Height);
        if (shorterSideResolution < CoverPhotoShorterSideResolution)
        {
            yield return CoverPhotoWrongResolution;
        }

        var aspectRatio = (double)originalVariant.Width / originalVariant.Height;
        if (aspectRatio + 0.001 < CoverPhotoMinRatio
            || aspectRatio - 0.001 > CoverPhotoMaxRatio)
        {
            yield return CoverPhotoWrongRatio;
        }

        if (!AllowedImageMimeTypes.Contains(originalVariant.MimeType))
        {
            yield return CoverPhotoWrongFormat;
        }
    }

    private static IEnumerable<Diagnostic> ValidateSubtitles(SubtitlesShardInfo subtitles)
    {
        if (!subtitles.Variants.ContainsKey(Const.OriginalShardVariant))
        {
            yield return MissingSubtitlesFile;
        }

        var originalVariant = subtitles.Variants[Const.OriginalShardVariant];

        if (originalVariant.IsCorrupted)
        {
            yield return SubtitlesCorrupted;
        }

        if (!AllowedSubtitleCodecs.Contains(originalVariant.Codec))
        {
            yield return SubtitlesWrongFormat;
        }
    }

    private static DiagnosticKind StatusToDiagnosticKind(string status)
    {
        switch (status)
        {
            case "INIT":
                return DiagnosticKind.Info;
            case "OK":
                return DiagnosticKind.Info;
            case "SKIPPED":
                return DiagnosticKind.Info;
            case "WARNING":
                return DiagnosticKind.Warning;
            case "ERROR":
                return DiagnosticKind.Error;
            case "CRASHED":
                return DiagnosticKind.Error;
            default:
                return DiagnosticKind.Info;
        }
    }


    private static IEnumerable<Diagnostic> ValidatePigeons(BlendShardInfo? blend)
    {
        if (blend is null)
        {
            yield break;
        }

        foreach (var variant in blend.Variants.Values)
        {
            if (variant.Error is not null)
            {
                yield return new Diagnostic(
                    Kind: DiagnosticKind.Error,
                    ValidationStage: FileStage,
                    Message: LocalizedString.Create(
                        (Const.InvariantCulture, $"PIGEOnS test could not be run: {variant.Error}"),
                        (Const.CzechCulture, $"PIGEOnS test nemohl být spuštěn: {variant.Error}")
                    )
                );
                yield break;
            }

            if (variant.Tests is null)
            {
                yield return MissingPigeonsTestResult;
                yield break;
            }
            foreach (var result in variant.Tests)
            {
                var sb = new System.Text.StringBuilder();
                if (!string.IsNullOrWhiteSpace(blend.FileName))
                {
                    sb.Append(blend.FileName);
                    sb.Append(" - ");
                }
                if (!string.IsNullOrWhiteSpace(result.Label))
                {
                    sb.Append(result.Label);
                }
                // Append additional info if any of the fields are present
                if (!string.IsNullOrWhiteSpace(result.Datablock) ||
                    !string.IsNullOrWhiteSpace(result.Message) ||
                    !string.IsNullOrWhiteSpace(result.Traceback))
                {
                    sb.Append(":");
                }
                else
                {
                    sb.Append(".");
                }
                if (!string.IsNullOrWhiteSpace(result.Datablock))
                {
                    sb.Append(" [");
                    sb.Append(result.Datablock);
                    sb.Append("]");
                }
                if (!string.IsNullOrWhiteSpace(result.Message))
                {
                    sb.Append(" ");
                    sb.Append(result.Message);
                }
                if (!string.IsNullOrWhiteSpace(result.Traceback))
                {
                    sb.Append(" ");
                    sb.Append("Traceback: ");
                    sb.Append(result.Traceback);
                }
                string message = sb.ToString();

                yield return new Diagnostic(
                    Kind: StatusToDiagnosticKind(result.State ?? "UNKNOWN"),
                    ValidationStage: FileStage,
                    Message: LocalizedString.Create(
                        (Const.InvariantCulture, message)
                    )
                );
            }
        }
    }
}
