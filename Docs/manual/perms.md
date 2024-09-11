# Permissions in KAFE

## `EntityPermissionInfo`

Complete descriptions of who has which permissions for an entity with specific `Id` and how they got 'em. 

**Parents**

Direct parent entities whose permissions affect the described entity.
Is important when the entity is reparented.

**Grantors**

All transitive parent entities whose permissions affect the described entity.
For example, if this entity is a project, _Grantors_ are the parent project group, the organization, and `system`.
Is important when propagating changes from parents to all of their descendants.


## `EntityPermissionEventProjection`

**Entity creation events**
- `OrganizationCreated`
- `ProjectGroupCreated`
- `ProjectCreated`
- `PlaylistCreated`
- `AuthorCreated`
- `ArtifactCreated`
- `AccountCreated`
- `RoleCreated`

**Entity change events**
- `ProjectGroupGlobalPermissionsChanged`
- `ProjectGroupMovedToOrganization`
- `ProjectGlobalPermissionsChanged`
- (`ProjectMovedToProjectGroup`)
- `ProjectArtifactAdded`
- `ProjectArtifactRemoved`
- `PlaylistGlobalPermissionsChanged`
- `PlaylistMovedToOrganization`


**Account changes**
- `AccountPermissionSet`
- `AccountPermissionUnset`
- `AccountRoleSet`
- `AccountRoleUnset`

**Role changes**
- `RolePermissionSet`
- `RolePermissionUnset`
