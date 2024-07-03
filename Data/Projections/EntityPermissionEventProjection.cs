using System.Threading.Tasks;
using Kafe.Data.Documents;
using Kafe.Data.Events;
using Marten;
using Marten.Events.Projections;

namespace Kafe.Data.Projections;

public class EntityPermissionEventProjection : EventProjection
{
    public EntityPermissionInfo Create(OrganizationCreated e)
    {
        return EntityPermissionInfo.Create(e.OrganizationId);
    }

    public EntityPermissionInfo Create(ProjectGroupCreated e)
    {
        var perms = EntityPermissionInfo.Create(e.ProjectGroupId);
        if (e.OrganizationId is not null)
        {
            perms = perms with
            {
                ParentIds = perms.ParentIds.Add(e.OrganizationId),
                GrantorIds = perms.GrantorIds.Add(e.OrganizationId)
            };
        }

        // TODO: Implement transitive perms from the org.

        return perms;
    }

    public async Task<EntityPermissionInfo> Create(ProjectCreated e, IDocumentOperations ops)
    {
        var groupPerms = await ops.LoadAsync<EntityPermissionInfo>(e.ProjectGroupId);
        var perms = EntityPermissionInfo.Create(e.ProjectId);
        perms = perms with
        {
            ParentIds = perms.ParentIds.Add(e.ProjectGroupId),
            GrantorIds = perms.GrantorIds.Add(e.ProjectGroupId)
        };

        if (groupPerms is not null)
        {
            // TODO: Implement transitive perms from the project group.
        }

        // TODO: Implement transitive perms from the org.

        return perms;
    }

    public EntityPermissionInfo Create(PlaylistCreated e)
    {
        var perms = EntityPermissionInfo.Create(e.PlaylistId);
        if (e.OrganizationId is not null)
        {
            perms = perms with
            {
                ParentIds = perms.ParentIds.Add(e.OrganizationId),
                GrantorIds = perms.GrantorIds.Add(e.OrganizationId)
            };
        }

        // TODO: Implement transitive perms from the org.

        return perms;
    }

    public EntityPermissionInfo Create(AuthorCreated e)
    {
        return EntityPermissionInfo.Create(e.AuthorId);
    }

    // public EntityPermissionInfo Create(ArtifactCreated e)
    // {
    //     var perms = EntityPermissionInfo.Create(e.ArtifactId);
    // }
}
