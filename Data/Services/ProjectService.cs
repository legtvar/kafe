using Kafe.Core.Diagnostics;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Kafe.Data.Metadata;
using Marten;
using Marten.Linq;
using Marten.Linq.MatchesSql;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public class ProjectService(
    IDocumentSession db,
    AccountService accountService,
    ArtifactService artifactService,
    EntityMetadataProvider entityMetadataProvider
)
{
    public async Task<Err<ProjectInfo>> Upsert(
        ProjectInfo project,
        Hrib? ownerId = null,
        ExistingEntityHandling existingEntityHandling = ExistingEntityHandling.Upsert,
        bool shouldOverrideLock = false,
        bool shouldSkipValidation = false,
        CancellationToken token = default
    )
    {
        var idErr = Hrib.TryParseValid(project.Id);
        if (idErr.HasError)
        {
            return idErr.Diagnostic.ForParameter(nameof(project.Id));
        }

        var id = idErr.Value;

        if (id.IsEmpty)
        {
            id = Hrib.Create();
        }

        var existingErr = await Load(id, token);
        switch (existingEntityHandling)
        {
            case ExistingEntityHandling.Update:
                if (existingErr.HasError)
                {
                    return existingErr;
                }

                break;
            case ExistingEntityHandling.Insert:
                if (existingErr.HasValue)
                {
                    return Err.Fail(new AlreadyExistsDiagnostic(typeof(ProjectInfo), id));
                }

                break;
        }

        var groupErr = await db.KafeLoadAsync<ProjectGroupInfo>(project.ProjectGroupId, token);
        if (groupErr.HasError)
        {
            return groupErr.Diagnostic;
        }

        var group = groupErr.Value;

        var artifactIdErr = Hrib.TryParseValid(project.ArtifactId);
        if (artifactIdErr.HasError)
        {
            return artifactIdErr.Diagnostic.ForParameter(nameof(project.ArtifactId));
        }

        var artifactId = artifactIdErr.Value;

        Err<ArtifactInfo> artifactErr;
        if (artifactId.IsEmpty)
        {
            artifactErr = await artifactService.Create(ArtifactInfo.Create(LocalizedString.Empty), token);
        }
        else
        {
            artifactErr = await artifactService.Load(artifactId, token);
        }

        if (artifactErr.HasError)
        {
            return artifactErr.Diagnostic.ForParameter(nameof(project.ArtifactId));
        }

        var artifact = artifactErr.Value;

        if (existingErr is { HasError: true, Diagnostic.Payload: NotFoundDiagnostic })
        {
            var created = new ProjectCreated(
                ProjectId: id.ToString(),
                ArtifactId: artifactId.ToString(),
                OwnerId: ownerId?.ToString(),
                CreationMethod: CreationMethod.Api,
                ProjectGroupId: group.Id
            );
            db.Events.KafeStartStream<ProjectInfo>(id, created);
            await db.SaveChangesAsync(token);
            existingErr = await db.Events.KafeAggregateStream<ProjectInfo>(id, token: token);
        }

        if (existingErr.HasError)
        {
            return existingErr.Diagnostic;
        }

        var existing = existingErr.Value;

        if (existing.IsLocked && !shouldOverrideLock)
        {
            return Err.Fail(new LockedDiagnostic(typeof(ProjectInfo), id));
        }

        if (!shouldSkipValidation)
        {
            // TODO: Implement project validation based on its blueprint (or rather implement that in
            //       ArtifactService and call it from here).
        }

        var eventStream = await db.Events.FetchForExclusiveWriting<ProjectInfo>(id.ToString(), token);

        if (project.IsLocked != existing.IsLocked)
        {
            eventStream.AppendOne(
                project.IsLocked
                    ? new ProjectLocked(id.ToString())
                    : new ProjectUnlocked(id.ToString())
            );
        }

        if (existing.ArtifactId != artifactId.ToString())
        {
            eventStream.AppendOne(
                new ProjectArtifactSet(id.ToString(), artifactId.ToString())
            );
        }

        await db.SaveChangesAsync(token);

        if (ownerId is not null)
        {
            await accountService.AddPermissions(
                ownerId,
                [(id, Permission.Read | Permission.Write | Permission.Append | Permission.Inspect)],
                token
            );
        }

        return await db.Events.KafeAggregateStream<ProjectInfo>(id, token: token);
    }

    /// <summary>
    /// Filter of projects.
    /// </summary>
    /// <param name="AccessingAccountId">
    /// <list type="bullet">
    /// <item> If null, doesn't filter by account access at all.</item>
    /// <item>
    ///     If <see cref="Hrib.Empty"/> assumes the account is an anonymous user
    ///     and filters only by global permissions.
    /// </item>
    /// <item> If <see cref="Hrib.Invalid"/>, throws an exception. </item>
    /// </list>
    /// </param>
    public record ProjectFilter(
        Hrib? ProjectGroupId = null,
        Hrib? OrganizationId = null,
        Hrib? AccessingAccountId = null
    );

    public async Task<ImmutableArray<ProjectInfo>> List(
        ProjectFilter? filter = null,
        string? sort = null,
        CancellationToken token = default
    )
    {
        filter ??= new ProjectFilter();

        var query = db.Query<ProjectInfo>();

        if (filter.AccessingAccountId is not null)
        {
            query = (IMartenQueryable<ProjectInfo>)query
                .WhereAccountHasPermission(
                    db.DocumentStore.Options.Schema,
                    Permission.Read,
                    filter.AccessingAccountId
                );
        }

        if (filter.ProjectGroupId is not null)
        {
            query = (IMartenQueryable<ProjectInfo>)query
                .Where(e => e.ProjectGroupId == filter.ProjectGroupId.ToString());
        }

        if (filter.OrganizationId is not null)
        {
            var sql =
                $"""
                 (
                     SELECT g.data ->> 'OrganizationId'
                     FROM {db.DocumentStore.Options.Schema.For<ProjectGroupInfo>()} AS g
                     WHERE g.id = d.data ->> 'ProjectGroupId'
                 ) = ?
                 """;
            query = (IMartenQueryable<ProjectInfo>)query
                .Where(p => p.MatchesSql(sql, filter.OrganizationId.ToString()));
        }

        if (!string.IsNullOrEmpty(sort))
        {
            query = (IMartenQueryable<ProjectInfo>)query.OrderBySortString(entityMetadataProvider, sort);
        }

        var results = (await query.ToListAsync(token)).ToImmutableArray();
        return results;
    }

    public async Task<Err<ProjectInfo>> Load(Hrib id, CancellationToken token = default)
    {
        return await db.KafeLoadAsync<ProjectInfo>(id, token);
    }

    public async Task<ImmutableArray<ProjectInfo>> LoadMany(IEnumerable<Hrib> ids, CancellationToken token = default)
    {
        return (await db.KafeLoadManyAsync<ProjectInfo>([.. ids], token)).Unwrap();
    }

    public Task<Err<bool>> Lock(Hrib projectId, CancellationToken token = default)
    {
        return SetLockStatus(projectId: projectId, shouldLock: true, token: token);
    }

    public Task<Err<bool>> Unlock(Hrib projectId, CancellationToken token = default)
    {
        return SetLockStatus(projectId: projectId, shouldLock: false, token: token);
    }

    private async Task<Err<bool>> SetLockStatus(Hrib projectId, bool shouldLock, CancellationToken token = default)
    {
        var stream = await db.Events.FetchForExclusiveWriting<ProjectInfo>(projectId.ToString(), token);
        if (stream.Aggregate is null)
        {
            return Err.Fail(new NotFoundDiagnostic(typeof(ProjectInfo), projectId));
        }

        if (shouldLock && !stream.Aggregate.IsLocked)
        {
            stream.AppendOne(new ProjectLocked(projectId.ToString()));
        }
        else if (!shouldLock && stream.Aggregate.IsLocked)
        {
            stream.AppendOne(new ProjectUnlocked(projectId.ToString()));
        }

        await db.SaveChangesAsync(token);
        return true;
    }

    public async Task<Err<bool>> AddReview(
        Hrib projectId,
        Hrib? reviewerId,
        ReviewKind kind,
        string reviewerRole,
        LocalizedString? comment,
        CancellationToken token = default
    )
    {
        var projectErr = await Load(projectId, token);
        if (projectErr.HasError)
        {
            return projectErr.Diagnostic;
        }

        db.Events.KafeAppend(
            projectId,
            new ProjectReviewAdded(
                ProjectId: projectId.ToString(),
                ReviewerId: reviewerId?.ToString(),
                Kind: kind,
                ReviewerRole: reviewerRole,
                Comment: comment
            )
        );
        await db.SaveChangesAsync(token);
        return true;
    }
}
