using System;
using System.Globalization;

namespace Kafe;

public static class Const
{
    public const string OriginalShardVariant = "original";
    public const string InvalidPath = "invalid";
    public const string InvalidFileExtension = ".invalid";
    public const string InvalidFormatName = "invalid";
    public const string InvalidEmailAddress = "invalid@example.com";
    public const string InvalidName = "Invalid";
    public const string InvalidMimeType = "invalid/invalid";
    public const long InvalidFileLength = -1;
    public const long ShardSizeLimit = 4_294_967_296;

    public const string TechReviewer = "tech";
    public const string VisualReviewer = "visual";
    public const string DramaturgyReviewer = "dramaturgy";

    public const string FilmBlueprintSlot = "film";
    public const string VideoAnnotationBlueprintSlot = "video-annotation";
    public const string CoverPhotoBlueprintSlot = "cover-photo";
    public const int CoverPhotoMinCount = 1;
    public const int CoverPhotoMaxCount = 5;

    public const string SystemName = "System";

    public const string TemporaryAccountPurpose = "TemporaryAccount";
    public const string EmailConfirmationPurpose = "EmailConfirmation";

    public static readonly TimeSpan AuthenticationCookieExpirationTime = new(30, 0, 0, 0);

    public const string MatroskaMimeType = "video/x-matroska";
    public const string MatroskaFileExtension = ".mkv";

    public const string Mp4MimeType = "video/mp4";
    public const string Mp4FileExtension = ".mp4";

    public const string BlendBlueprintSlot = "blend";
    public const string BlendMimeType = "application/x-blender";
    public const string BlendFileExtension = ".blend";

    public const string TextureBlueprintSlot = "texture";
    public const string RenderedImageBlueprintSlot = "rendered-image";
    public const string RenderedAnimationBlueprintSlot = "rendered-animation";

    public const string InvariantCultureCode = "iv";
    public static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

    public const string EnglishCultureName = "en";
    public static readonly CultureInfo EnglishCulture;

    public const string CzechCultureName = "cs";
    public static readonly CultureInfo CzechCulture;

    public const string SlovakCultureName = "sk";
    public static readonly CultureInfo SlovakCulture;

    public const string CzechOrSlovakPseudoCultureName = $"{CzechCultureName}|{SlovakCultureName}";

    public static readonly LocalizedString UnknownAuthor;
    public static readonly LocalizedString UnknownProjectGroup;
    public static readonly LocalizedString ConfirmationEmailSubject;
    public static readonly LocalizedString InvitationEmailSubject;
    public static readonly LocalizedString ConfirmationEmailMessageTemplate;
    public static readonly LocalizedString InvitationTemplate;
    public static readonly LocalizedString InvitationGenericTemplate;
    public static readonly LocalizedString InvitationEmailMessageTemplate;
    public static readonly LocalizedString ProjectReviewEmailSubject;
    public static readonly LocalizedString[] EmailSignOffs;
    public static readonly LocalizedString CzechLanguageName;
    public static readonly LocalizedString CzechOrSlovakLanguageName;
    public static readonly LocalizedString InvariantLanguageName;
    public static readonly LocalizedString EnglishLanguageName;

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
        InvitationEmailSubject = LocalizedString.Create(
            (InvariantCulture, "Invitation to join KAFE"),
            (CzechCulture, "Přihlaš se do KAFE"),
            (SlovakCulture, "Prihlás sa do KAFE"));
        ProjectReviewEmailSubject = LocalizedString.Create(
            (InvariantCulture, "Project Review"),
            (CzechCulture, "Posouzení projektu"),
            (SlovakCulture, "Posúdenie projektu"));

        ConfirmationEmailMessageTemplate = LocalizedString.Create(
            (InvariantCulture,
@"Hello,

Please click the following one-time link to log into KAFE:

{0}

{1}

Yours,
KAFE
"),
            (CzechCulture,
@"Dobrý den,

prosím klikněte na následující jednorázový odkaz pro přihlášení do KAFE:

{0}

{1}

Vaše KAFE
"),
            (SlovakCulture,
@"Dobrý deň,

prosím kliknite na nasledujúci jednorazový odkaz pre prihlásenie do KAFE:

{0}

{1}

Vaše KAFE
"));

InvitationTemplate = LocalizedString.Create(
    (InvariantCulture, @"{0} invited you to KAFE!"),
    (CzechCulture, @"{0} Vás pozval(a) do KAFE!"),
    (SlovakCulture, @"{0} Vás pozval(a) do KAFE!"));

InvitationGenericTemplate = LocalizedString.Create(
    (InvariantCulture, @"You have been invited to KAFE!"),
    (CzechCulture, @"Byli jste pozváni do KAFE!"),
    (SlovakCulture, @"Boli ste pozvaní do KAFE!"));

InvitationEmailMessageTemplate = LocalizedString.Create(
            (InvariantCulture,
@"Hello,

{0}

Please click the link to activate your account:

{1}

{2}

Yours,
KAFE
"),
            (CzechCulture,
@"Dobrý den,

{0}

Prosím klikněte na následující odkaz pro přihlášení do KAFE:

{1}

{2}

Vaše KAFE
"),
            (SlovakCulture,
@"Dobrý deň,

{0}

Prosím kliknite na nasledujúci odkaz pre prihlásenie do KAFE:

{1}

{2}

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
                (SlovakCulture, "Do nekonečna a ešte ďalej!")),
            LocalizedString.Create(
                (InvariantCulture, "Have fun out there."),
                (CzechCulture, "Užijte si to."),
                (SlovakCulture, "Užite si to."))
        };

        CzechLanguageName = LocalizedString.Create(
            (InvariantCulture, "Czech"),
            (CzechCulture, "čeština")
        );

        CzechOrSlovakLanguageName = LocalizedString.Create(
            (InvariantCulture, "Czech or Slovak"),
            (CzechCulture, "čeština nebo slovenština")
        );

        EnglishLanguageName = LocalizedString.Create(
            (InvariantCulture, "English"),
            (CzechCulture, "angličtina")
        );

        InvariantLanguageName = LocalizedString.Create(
            (InvariantCulture, "international"),
            (CzechCulture, "mezinárodní")
        );
    }

    public static LocalizedString GetLanguageName(string cultureCode)
    {
        if (cultureCode == InvariantCultureCode)
        {
            return InvariantLanguageName;
        }

        if (cultureCode == CzechCultureName)
        {
            return CzechLanguageName;
        }

        if (cultureCode == CzechOrSlovakPseudoCultureName)
        {
            return CzechOrSlovakLanguageName;
        }

        if (cultureCode == EnglishCultureName)
        {
            return EnglishLanguageName;
        }

        return LocalizedString.CreateInvariant(cultureCode);
    }
}
