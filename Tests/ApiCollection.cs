using Xunit;

namespace Kafe.Tests;

[CollectionDefinition(Const.Collections.Api, DisableParallelization = true)]
public class ApiCollection : ICollectionFixture<ApiFixture>
{ }
