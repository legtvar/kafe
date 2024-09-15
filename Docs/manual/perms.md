# Permissions in KAFE

An `Account` or a `Role` can have any of the following permissions:

- `None` - You can't see and do nothing.
- `Read` - You can view the entity's metadata.
- `Append` - You can modify the list of the entity's children but only non-destructively.
- † `Inspect` - Gives `Read` and `Inspect` on all descendant entities.
- † `Write` - You can modify the entity and any of its descendants.
- † `Review` - You can send a review to a project owner. (Only makes sense on projects.)
- † `Administer` - You can edit the entity's global and explicit permissions.
- † `Inheritable` = `Inspect | Write | Review | Administer`
- `All` - All of the above.

Permissions with the dagger symbol (†) are inheritable -- have effects on the entity's descendants -- the others concern only the entity itself.

## Explicit and inherited permissions

Permissions can be either explicit for a specific entity or trickle down from its ancestors:

```
system
- authors
- accounts
- organizations
    - playlists
    - roles
    - project groups
        - projects
            - artifacts
                - shards
```

- The `system` entity is special. It rules all. Permissions for the `system` entity are "inherited" by all others.
- Permissions are always additive. If one can explictly `Inspect` and organization and `Write` to one of its project groups, one inherited the `Read` and `Write` to all of the project group's projects.
- Playlists are just lists. That's it. Especially, playlists are not _parents_ of any artifacts. No permissions trickle down from playlists to artifacts.
- There is no _administrator_ role in KAFE. Administrators are accounts with an explicit `All` permission on the `system` entity.

## `EntityPermissionInfo`

Complete descriptions of who has which permissions for an entity with specific `Id` and how they got 'em. 

**Parents**

Direct parent entities whose permissions affect the described entity.
It is important when the entity is reparented.

**Grantors**

All transitive parent entities whose permissions affect the described entity.
For example, if this entity is a project, _Grantors_ are the parent project group, the organization, and `system`.
It is important when propagating changes from parents to all of their descendants.


## `EntityPermissionEventProjection`

The core of the KAFE's permission system.
Creates and changes `EntityPermissionInfo` documents.
Each subset of events has specific effects:

### Entity creation events

```csharp
OrganizationCreated (OrganizationId,    ...)
ProjectGroupCreated (ProjectGroupId,    ...)
ProjectCreated      (ProjectId,         ...)
PlaylistCreated     (PlaylistId,        ...)
AuthorCreated       (AuthorId,          ...)
ArtifactCreated     (ArtifactId,        ...)
AccountCreated      (AccountId,         ...)
RoleCreated         (RoleId,            ...)
```

When any of the events above is recorded, a new `EntityPermissionInfo` is created.
Those accounts with inheritable permission to any ancestor entity (or `system`) are given those inheritable permissions to this entity as well.


### Explicit `Account` or `Role` permissions

```csharp
AccountPermissionSet    (AccountId, EntityId, Permission);
RolePermissionSet       (RoleId,    EntityId, Permission);
```

When any of the events above is recorded, a new explicit `Permission` is assigned to `EntityPermissionInfo` for `EntityId` for the specified `AccountId` or `RoleId`.
If `Permission` (or any of its flags) is inheritable it is _spread_ (using the list of _grantors_) to all descendant entities of `EntityId`. 
With role permission changes, `EntityPermissionInfo` gains an entry for the role and an entry for each member of that role.


### Global permission changes

```csharp
ProjectGroupGlobalPermissionsChanged    (ProjectGroupId,    GlobalPermissions)
ProjectGlobalPermissionsChanged         (ProjectId,         GlobalPermissions)
PlaylistGlobalPermissionsChanged        (PlaylistId,        GlobalPermissions)
```

Project groups, projects, and playlist can have _global permissions_.
These permissions apply to all users with accounts **and also anonymous users**.
They are limited to `Permission.Publishable` which includes `Read`, `Inspect`, and `Append`.
These perms are not recorded as entries but are instead saved into a special `GlobalPermission` property that **must** be added to any explicit or inherited account or role permissions.

### Moves

- `ProjectGroupMovedToOrganization`
- (`ProjectMovedToProjectGroup`)
- `ProjectArtifactAdded`
- `ProjectArtifactRemoved`
- `PlaylistMovedToOrganization`

### Role assignments

- `AccountRoleSet`
- `AccountRoleUnset`
