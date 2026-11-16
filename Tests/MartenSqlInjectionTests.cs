using System.Linq;
using System.Threading.Tasks;
using Kafe.Data.Aggregates;
using Marten;
using Xunit;

namespace Kafe.Tests;

[Collection(Const.Collections.Api)]
public class MartenSqlInjectionTests(ApiFixture fixture, ITestOutputHelper testOutput)
    : ApiTestBase(fixture, testOutput)
{
    [Theory]
    [InlineData("' OR TRUE; --")]
    [InlineData("'; SELECT 1; --")]
    [InlineData("' OR '1' = '1")]
    [InlineData("O'Connor@example.com")]
    public async Task LinqStringLiteralFilter_WithQuotesAndSqlSyntax_ShouldMatchOnlyLiteralValue(
        string emailFilter
    )
    {
        var expected = AccountInfo.Create(emailFilter) with { Id = "sqlinj0001" };
        var other = AccountInfo.Create("other@example.com") with { Id = "sqlinj0002" };

        await using (var session = Store.LightweightSession())
        {
            session.Store(expected, other);
            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var query = Store.QuerySession();
        var matches = await query.Query<AccountInfo>()
            .Where(a => a.EmailAddress == emailFilter)
            .ToListAsync(TestContext.Current.CancellationToken);

        var match = Assert.Single(matches);
        Assert.Equal(expected.Id, match.Id);
        Assert.Equal(emailFilter, match.EmailAddress);
    }
}
