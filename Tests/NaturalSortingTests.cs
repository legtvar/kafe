using System;
using System.Linq;
using System.Threading.Tasks;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Metadata;
using Marten;
using Marten.Linq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kafe.Tests;

[Collection(Const.Collections.Api)]
public class NaturalSortingTests(ApiFixture fixture, ITestOutputHelper testOutput)
    : ApiTestBase(fixture, testOutput)
{
    private const string TenId = "natural0001";
    private const string TwoId = "natural0002";
    private const string OneId = "natural0003";

    [Fact]
    public async Task ProjectNames_UseNaturalSorting()
    {
        await AssertNameOrder(
            (id, name) => ProjectInfo.Create(TestSeedData.Group1Hrib, LocalizedString.CreateInvariant(name)) with
            {
                Id = id
            }
        );
    }

    [Fact]
    public async Task ProjectGroupNames_UseNaturalSorting()
    {
        await AssertNameOrder(
            (id, name) => ProjectGroupInfo.Create(TestSeedData.Org1Hrib, LocalizedString.CreateInvariant(name)) with
            {
                Id = id
            }
        );
    }

    [Fact]
    public async Task PlaylistNames_UseNaturalSorting()
    {
        await AssertNameOrder(
            (id, name) => PlaylistInfo.Create(TestSeedData.Org1Hrib, LocalizedString.CreateInvariant(name)) with
            {
                Id = id
            }
        );
    }

    [Fact]
    public async Task OrganizationNames_KeepLexicalSorting()
    {
        await AssertNameOrder(
            (id, name) => OrganizationInfo.Create(LocalizedString.CreateInvariant(name)) with { Id = id },
            expectNaturalSorting: false
        );
    }

    private async Task AssertNameOrder<T>(
        Func<string, string, T> create,
        bool expectNaturalSorting = true
    ) where T : IEntity
    {
        var entities = new[]
        {
            create(TenId, "10. Filmový festival FI MU"),
            create(TwoId, "2. Filmový festival FI MU"),
            create(OneId, "1. Filmový festival FI MU")
        };
        var ids = entities.Select(e => e.Id).ToArray();

        await using (var session = Store.LightweightSession())
        {
            session.Store(entities);
            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var query = Store.QuerySession();
        var metadataProvider = Host.Server.Services.GetRequiredService<EntityMetadataProvider>();
        var ordered = await query.Query<T>()
            .Where(e => ids.Contains(e.Id))
            .OrderBySortString(metadataProvider, "name.iv")
            .ToListAsync(TestContext.Current.CancellationToken);

        if (!expectNaturalSorting)
        {
            // PostgreSQL's default ordering of punctuation depends on the database locale.
            var sortExpression = metadataProvider.GetSortableMetadata(typeof(T)).SortExpressions["name.iv"];
            var defaultOrdered = await query.Query<T>()
                .Where(e => ids.Contains(e.Id))
                .OrderBySql(sortExpression)
                .ToListAsync(TestContext.Current.CancellationToken);
            Assert.Equal(defaultOrdered.Select(e => e.Id), ordered.Select(e => e.Id));
            return;
        }

        Assert.Equal([OneId, TwoId, TenId], ordered.Select(e => e.Id));
    }
}
