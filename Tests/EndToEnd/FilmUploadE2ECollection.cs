using Xunit;

namespace Kafe.Tests.EndToEnd;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class FilmUploadE2ECollection : ICollectionFixture<FilmUploadE2EFixture>
{
    public const string Name = "Film upload E2E";
}
