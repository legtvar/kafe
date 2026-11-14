using Xunit;

namespace Kafe.EndToEnd;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class FilmSubmissionEndToEndCollection : ICollectionFixture<EndToEndFixture>
{
    public const string Name = "Film upload E2E";
}
