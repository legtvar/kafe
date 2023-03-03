using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe;

public static class Const
{
    public const string OriginalShardVariant = "original";
    public const string InvalidPath = "invalid";
    public const string InvalidFileExtension = "invalid";
    public const string InvalidFormatName = "invalid";
    public const long ShardSizeLimit = 4_294_967_296;

    public const string MatroskaMimeType = "video/x-matroska";
    public const string MatroskaFileExtension = ".mkv";

    public const string Mp4MimeType = "video/mp4";
    public const string Mp4FileExtension = ".mp4";

    public const string InvariantCultureCode = "iv";
    public static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

    public const string EnglishCultureName = "en";
    public static readonly CultureInfo EnglishCulture = CultureInfo.CreateSpecificCulture(EnglishCultureName);

    public const string CzechCultureName = "cz";
    public static readonly CultureInfo CzechCulture = CultureInfo.CreateSpecificCulture(CzechCultureName);

    public const string SlovakCultureName = "sk";
    public static readonly CultureInfo SlovakCulture = CultureInfo.CreateSpecificCulture(SlovakCultureName);

    public static readonly LocalizedString UnknownAuthor
        = LocalizedString.Create(
            (InvariantCulture, "Unknown author"),
            (CzechCulture, "Neznámý autor"));

    public static readonly LocalizedString UnknownProjectGroup
        = LocalizedString.Create(
            (InvariantCulture, "Unknown project group"),
            (CzechCulture, "Neznámá skupina projektů"));

    public static readonly LocalizedString ConfirmationEmailSubject
        = LocalizedString.Create(
            (InvariantCulture, "[KAFE] Account Created"),
            (CzechCulture, "[KAFE] Účet vytvořen"),
            (SlovakCulture, "[KAFE] Účet vytvorený"));

    public static readonly LocalizedString ConfirmationEmailMessageTemplate
        = LocalizedString.Create(
            (InvariantCulture,
@"Hello,

Please click the following link to log into KAFE:

{}

{}

Yours,
KAFE
"),
            (CzechCulture,
@"Dobrý den,

Prosím klikněte na následující odkaz pro přihlášení do KAFE:

{}

{}

Vaše KAFE
"),
            (SlovakCulture,
@"Dobrý deň,

Prosím kliknite na nasledujúci odkaz pre prihlásenie do KAFE:

{}

{}

Vaše KAFE
"));

    public static readonly LocalizedString[] EmailSignOffs = new LocalizedString[]
    {
        LocalizedString.Create(
            (InvariantCulture, "Live long and prosper."),
            (CzechCulture, "Žijte dlouho a blaze."),
            (SlovakCulture, "Žite dlho a blaho.")),
        LocalizedString.Create(
            (InvariantCulture, "May the Force be with you."),
            (CzechCulture, "Ať je Síla s Vámi."),
            (SlovakCulture, "Nech Vás Sila sprevádza.")),
        LocalizedString.Create(
            (InvariantCulture, "So long, and thanks for all the fish."),
            (CzechCulture, "Sbohem a díky za všechny ryby."),
            (SlovakCulture, "Zbohom a ďakujeme za ryby")),
        LocalizedString.Create(
            (InvariantCulture, "To infinity and beyond!"),
            (CzechCulture, "Do nekonečna a ještě dál!"),
            (SlovakCulture, "Do nekonečna a ešte ďalej!"))
    };
}
