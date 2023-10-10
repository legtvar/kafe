using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Capabilities;
using Kafe.Data.Events;
using Kafe.Data.Services;
using Marten;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public class AuthorService
{
    private readonly IDocumentSession db;
    private readonly IUserProvider userProvider;
    private readonly AccountService accountService;

    public AuthorService(
        IDocumentSession db,
        IUserProvider userProvider,
        AccountService accountService)
    {
        this.db = db;
        this.userProvider = userProvider;
        this.accountService = accountService;
    }

    public async Task<AuthorInfo> Create(AuthorCreationDto dto, Hrib? ownerId, CancellationToken token = default)
    {
        var created = new AuthorCreated(
            AuthorId: Hrib.Create(),
            CreationMethod: CreationMethod.Api,
            Name: dto.Name,
            Visibility: dto.Visibility);
        db.Events.StartStream<AuthorInfo>(created.AuthorId, created);
        if (!string.IsNullOrEmpty(dto.Uco)
            || LocalizedString.IsNullOrEmpty(dto.Bio)
            || !string.IsNullOrEmpty(dto.Email)
            || !string.IsNullOrEmpty(dto.Phone))
        {
            var infoChanged = new AuthorInfoChanged(
                AuthorId: created.AuthorId,
                Bio: (ImmutableDictionary<string, string>?)dto.Bio,
                Uco: dto.Uco,
                Email: dto.Email,
                Phone: dto.Phone);
            db.Events.Append(created.AuthorId, infoChanged);
        }
        await db.SaveChangesAsync(token);

        if (ownerId is not null)
        {
            await accountService.AddPermissions(
                ownerId,
                new [] { (created.AuthorId, Permission.All) },
                token);
        }

        var author = await db.Events.AggregateStreamAsync<AuthorInfo>(created.AuthorId, token: token);
        if (author is null)
        {
            throw new InvalidOperationException($"Could not persist an author with id '{created.AuthorId}'.");
        }

        return author;
    }

    public async Task<ImmutableArray<AuthorListDto>> List(CancellationToken token = default)
    {
        var authors = await db.Query<AuthorInfo>()
            .WhereCanRead(userProvider)
            .ToListAsync(token);
        return authors.Select(TransferMaps.ToAuthorListDto).ToImmutableArray();
    }

    public async Task<AuthorDetailDto?> Load(Hrib id, CancellationToken token = default)
    {
        var author = await db.LoadAsync<AuthorInfo>(id, token);
        if (author is null)
        {
            return null;
        }

        if (!userProvider.CanRead(author))
        {
            throw new UnauthorizedAccessException();
        }

        return TransferMaps.ToAuthorDetailDto(author);
    }
}
