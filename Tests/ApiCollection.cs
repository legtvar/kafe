using Xunit;

namespace Kafe.Tests;

[CollectionDefinition(Const.Collections.Api)]
public class ApiCollection : ICollectionFixture<ApiFixture>
{ }
