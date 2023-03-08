using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Kafe.Data.Options;
using Kafe.Media.Services;
using Marten;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class DefaultArtifactService : IArtifactService
{
    private readonly IDocumentSession db;
    private readonly IOptions<StorageOptions> storageOptions;
    private readonly IMediaService mediaService;
    private readonly IUserProvider userProvider;

    public DefaultArtifactService(
        IDocumentSession db,
        IOptions<StorageOptions> storageOptions,
        IMediaService mediaService,
        IUserProvider userProvider)
    {
        this.db = db;
        this.storageOptions = storageOptions;
        this.mediaService = mediaService;
        this.userProvider = userProvider;
    }

    public async Task<ArtifactDetailDto?> Load(Hrib id, CancellationToken token = default)
    {
        if (!userProvider.IsAdministrator())
        {
            throw new UnauthorizedAccessException();
        }

        var artifact = await db.LoadAsync<ArtifactDetail>(id, token);
        if (artifact is null)
        {
            return null;
        }

        return TransferMaps.ToArtifactDetailDto(artifact);
    }

    public async Task<ImmutableArray<ArtifactDetailDto?>> LoadMany(IEnumerable<Hrib> ids, CancellationToken token = default)
    {
        if (!userProvider.IsAdministrator())
        {
            throw new UnauthorizedAccessException();
        }

        var artifacts = await db.LoadManyAsync<ArtifactDetail>(token, ids.Select(i => (string)i));
        return artifacts.Select(a => a is null ? null : TransferMaps.ToArtifactDetailDto(a))
            .ToImmutableArray();
    }

    public async Task<Hrib> Create(ArtifactCreationDto dto, CancellationToken token = default)
    {
        var containingProject = await db.LoadAsync<ProjectInfo>(dto.ContainingProject, token);
        if (containingProject is null)
        {
            throw new ArgumentException($"Project '{dto.ContainingProject}' does not exist.");
        }

        if (!userProvider.CanEdit(containingProject))
        {
            throw new UnauthorizedAccessException($"The '{dto.ContainingProject}' project is not editable.");
        }

        var created = new ArtifactCreated(
            ArtifactId: Hrib.Create(),
            CreationMethod: CreationMethod.Api,
            Name: dto.Name,
            AddedOn: dto.AddedOn?.ToUniversalTime() ?? DateTimeOffset.UtcNow);
        db.Events.StartStream<ArtifactInfo>(created.ArtifactId, created);

        if (dto.ContainingProject is not null)
        {
            var artifactAdded = new ProjectArtifactAdded(
                ProjectId: dto.ContainingProject,
                ArtifactId: created.ArtifactId,
                BlueprintSlot: dto.BlueprintSlot);
            var projectStream = await db.Events.FetchForWriting<ProjectInfo>(dto.ContainingProject, token);
            projectStream.AppendOne(artifactAdded);
        }

        await db.SaveChangesAsync(token);
        return created.ArtifactId;
    }
}
