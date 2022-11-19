using System.Collections.Immutable;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record Project(
    string Id,
    CreationMethod CreationMethod,
    string ProjectGroupId,
    ImmutableArray<ProjectAuthor> Authors,
    string? Name = null,
    string? Description = null,
    string? EnglishName = null,
    string? EnglishDescription = null,
    Visibility Visibility = Visibility.Unknown,
    DateTimeOffset ReleaseDate = default,
    string? Link = null
);

public record ProjectAuthor(
    string Id,
    ImmutableArray<string> Jobs
);

public class ProjectProjection : SingleStreamAggregation<Project>
{
    public ProjectProjection()
    {
    }

    public Project Create(IEvent<ProjectCreated> e)
    {
        return new Project(
            Id: e.StreamKey!,
            CreationMethod: e.Data.CreationMethod,
            ProjectGroupId: e.Data.ProjectGroupId,
            Authors: ImmutableArray.Create<ProjectAuthor>());
    }
    
    public Project Apply(ProjectInfoChanged e, Project p)
    {
        return p with {
            Name = e.Name,
            Description = e.Description,
            EnglishName = e.EnglishName,
            EnglishDescription = e.EnglishDescription,
            Visibility = e.Visibility,
            ReleaseDate = e.ReleaseDate,
            Link = e.Link
        };
    }

    public Project Apply(ProjectAuthorAdded e, Project p)
    {
        if (p.Authors.IsDefault)
        {
            p = p with { Authors = ImmutableArray.Create<ProjectAuthor>() };
        }

        var author = p.Authors.SingleOrDefault(a => a.Id == e.AuthorId);
        if (author is not null && e.Jobs.HasValue && !e.Jobs.Value.IsDefault)
        {
            author.Jobs
                .Union(e.Jobs)
                .ToImmutableArray();
        }
        else
        {
            author = new ProjectAuthor(e.AuthorId, e.Jobs.HasValue && !e.Jobs.Value.IsDefault
                ? e.Jobs.Value : ImmutableArray.Create<string>());
        }

        return p with
        {
            Authors = p.Authors.RemoveAll(a => a.Id == e.AuthorId)
                .Add(author)
        };
    }
}
