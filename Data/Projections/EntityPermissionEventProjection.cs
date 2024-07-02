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
                ParentEntityIds = perms.ParentEntityIds.Add(e.OrganizationId)
            };
        }

        return perms;
    }

    public async Task<EntityPermissionInfo> Create(ProjectCreated e, IDocumentOperations ops)
    {
        var groupPerms = await ops.LoadAsync<EntityPermissionInfo>(e.ProjectGroupId);
        var perms = EntityPermissionInfo.Create(e.ProjectId);
        perms = perms with
        {
            ParentEntityIds = perms.ParentEntityIds.Add(e.ProjectGroupId)
        };

        if (groupPerms is not null)
        {
            perms = perms with
            {
                ParentEntityIds = perms.ParentEntityIds.Union(groupPerms.ParentEntityIds)
            };
        }

        return perms;
    }

    public EntityPermissionInfo Create(PlaylistCreated e)
    {
        var perms = EntityPermissionInfo.Create(e.PlaylistId);
        if (e.OrganizationId is not null)
        {
            perms = perms with
            {
                ParentEntityIds = perms.ParentEntityIds.Add(e.OrganizationId)
            };
        }

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
