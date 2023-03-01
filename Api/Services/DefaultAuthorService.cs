using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class DefaultAuthorService : IAuthorService
{
    private readonly IDocumentSession db;

    public DefaultAuthorService(IDocumentSession db)
    {
        this.db = db;
    }

    public async Task<Hrib> Create(AuthorCreationDto dto, CancellationToken token = default)
    {
        var created = new AuthorCreated(
            AuthorId: Hrib.Create(),
            CreationMethod: CreationMethod.Api,
            Name: dto.Name);
        db.Events.StartStream<AuthorInfo>(created.AuthorId, created);
        if (!string.IsNullOrEmpty(dto.Uco)
            || LocalizedString.IsNullOrEmpty(dto.Bio)
            || !string.IsNullOrEmpty(dto.Email)
            || !string.IsNullOrEmpty(dto.Phone))
        {
            var infoChanged = new AuthorInfoChanged(
                AuthorId: created.AuthorId,
                Bio: dto.Bio,
                Uco: dto.Uco,
                Email: dto.Email,
                Phone: dto.Phone);
            db.Events.Append(created.AuthorId, infoChanged);
        }

        await db.SaveChangesAsync(token);
        return created.AuthorId;
    }

    public async Task<ImmutableArray<AuthorListDto>> List(CancellationToken token = default)
    {
        var authors = await db.Query<AuthorInfo>().ToListAsync(token);
        return authors.Select(TransferMaps.ToAuthorListDto).ToImmutableArray();
    }

    public async Task<AuthorDetailDto?> Load(Hrib id, CancellationToken token = default)
    {
        var author = await db.LoadAsync<AuthorInfo>(id, token);
        return author is null
            ? null
            : TransferMaps.ToAuthorDetailDto(author);
    }
}
