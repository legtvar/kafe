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
    public const string InvalidEmailAddress = "invalid@example.com";
    public const string InvalidName = "Invalid";
    public const long InvalidFileLength = -1;
    public const long ShardSizeLimit = 4_294_967_296;

    public const string SystemName = "System";

    public static readonly TimeSpan AuthenticationCookieExpirationTime = new(30, 0, 0, 0);

    public const string MatroskaMimeType = "video/x-matroska";
    public const string MatroskaFileExtension = ".mkv";

    public const string Mp4MimeType = "video/mp4";
    public const string Mp4FileExtension = ".mp4";

    public const string InvariantCultureCode = "iv";
    public static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

    public const string EnglishCultureName = "en";
    public static readonly CultureInfo EnglishCulture;

    public const string CzechCultureName = "cs";
    public static readonly CultureInfo CzechCulture;

    public const string SlovakCultureName = "sk";
    public static readonly CultureInfo SlovakCulture;


    public static readonly LocalizedString UnknownAuthor;
    public static readonly LocalizedString UnknownProjectGroup;
    public static readonly LocalizedString ConfirmationEmailSubject;
    public static readonly LocalizedString ConfirmationEmailMessageTemplate;
    public static readonly LocalizedString[] EmailSignOffs;

    static Const()
    {
        EnglishCulture = CultureInfo.CreateSpecificCulture(EnglishCultureName);
        CzechCulture = CultureInfo.CreateSpecificCulture(CzechCultureName);
        SlovakCulture = CultureInfo.CreateSpecificCulture(SlovakCultureName);

        UnknownAuthor = LocalizedString.Create(
            (InvariantCulture, "Unknown author"),
            (CzechCulture, "Neznámý autor"));
        UnknownProjectGroup = LocalizedString.Create(
            (InvariantCulture, "Unknown project group"),
            (CzechCulture, "Neznámá skupina projektů"));
        ConfirmationEmailSubject = LocalizedString.Create(
            (InvariantCulture, "Login Request"),
            (CzechCulture, "Přihlášení"),
            (SlovakCulture, "Prihlásenie"));
        ConfirmationEmailMessageTemplate = LocalizedString.Create(
            (InvariantCulture,
@"Hello,

Please click the following link to log into KAFE:

{0}

{1}

Yours,
KAFE
"),
            (CzechCulture,
@"Dobrý den,

prosím klikněte na následující odkaz pro přihlášení do KAFE:

{0}

{1}

Vaše KAFE
"),
            (SlovakCulture,
@"Dobrý deň,

prosím kliknite na nasledujúci odkaz pre prihlásenie do KAFE:

{0}

{1}

Vaše KAFE
"));
        EmailSignOffs = new LocalizedString[]
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
}
