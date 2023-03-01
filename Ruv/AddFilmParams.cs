using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Ruv;

public record AddFilmParams(
    int Year,
    RegisterArtworkArtworkType ArtworkType,
    bool IsFestivalWinner,
    string NameCS,
    ImmutableArray<string> KeywordsCS,
    string AnnotationCS,
    string NameEN,
    ImmutableArray<string> KeywordsEN,
    string AnnotationEN,
    ImmutableArray<SaveAuthorParams> Authors,
    DateTimeOffset FestivalDate,
    string ImagePath,
    string? CitationLink = null,
    string StudyProgram = RuvConst.DefaultStudyProgram,
    string StudySubject = RuvConst.DefaultStudySubject);
