﻿using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Capabilities;
using Kafe.Data.Events;
using Marten;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class DefaultAuthorService : IAuthorService
{
    private readonly IDocumentSession db;
    private readonly IUserProvider userProvider;
    private readonly IAccountService accountService;

    public DefaultAuthorService(
        IDocumentSession db,
        IUserProvider userProvider,
        IAccountService accountService)
    {
        this.db = db;
        this.userProvider = userProvider;
        this.accountService = accountService;
    }

    public async Task<Hrib> Create(AuthorCreationDto dto, CancellationToken token = default)
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

        if (userProvider.User is not null)
        {
            await accountService.AddCapabilities(
                userProvider.User.Id,
                new[] { new AuthorManagement(created.AuthorId) },
                token);
            await userProvider.Refresh(token: token);
        }

        return created.AuthorId;
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
