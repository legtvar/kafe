using Kafe.Ruv;
using System.Collections.Immutable;

var ruv = new RuvClient();
await ruv.LogIn("username", "password");

var timeyFimey = new AddFilmParams(
    Year: 2022,
    ArtworkType: RegisterArtworkArtworkType.Actor,
    IsFestivalWinner: false,
    NameCS: "Timey FImey",
    KeywordsCS: ImmutableArray.Create("fi", "cestování časem"),
    AnnotationCS: "Dva studenti objeví revoluční způsob, jak se učit na Algo.",
    NameEN: "Timey FImey",
    KeywordsEN: ImmutableArray.Create("fi", "time travel"),
    AnnotationEN: "Two students discover a revolutionary new way to study for Algo.",
    Authors: ImmutableArray.Create(new SaveAuthorParams(
        PersonalNumber: "personalnumber",
        FirstName: "Adam",
        LastName: "Štěpánek",
        DegreeAfterName: "Bc.")),
    FestivalDate: new DateTimeOffset(2022, 12, 1, 19, 30, 00, TimeSpan.FromHours(1)),
    ImagePath: @"logo");

await ruv.AddFilm(timeyFimey);
