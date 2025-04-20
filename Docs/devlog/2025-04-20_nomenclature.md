# Nomenclature

There's a lot of things that need to be named.

While chipping away on the humonguous [#194 Artifact overhaul](https://gitlab.fi.muni.cz/legtvar/kafe/-/issues/194)
issue, I decided on a nomenclature for code elements related to naming.

**Id**

- Automatically-assigned unique name for a specific **instance** of an entity.
- Always a `Hrib`. (Unless it's a Marten document, then it has to be a `string` with an explicit implementation
  of `IEntity.Id`. I've opened up [an issue](https://github.com/JasperFx/marten/issues/3765) in Marten,
  hoping to resolve this.)
- Typically looks like this:
  ```csharp
  public Hrib Id { get; }
  ```

**Name**

- User-provided name for a specific **instance** of an entity.
- Always a `LocalizedString` (maybe with the exception of actual people's names).
- Typically looks like this:
  ```csharp
  public LocalizedString Name { get; } = LocalizedString.Create(...);
  ```

**Title**

- Human-readable name for specific **type** (e.g., an entity or a diagnostic payload).
- Always `LocalizedString`.
- Read through reflection when the type is registered through `ModContext`.
- It isn't convention-based. It's only read if the type implements an interface with the definition of this property
  (e.g., `IEntity` or `IDiagnosticPayload`).
- Typically looks like this:

  ```csharp
  public static LocalizedString Title { get; } = LocalizedString.Create(...);

  static IEntity.Title { get; } = LocalizedString.Create(...);

  static IDiagnosticPayload.Title { get; } = LocalizedString.Create(...);
  ```

**Moniker**

- The name of the `KafeType`.
- Always a `string`.
- Becomes `KafeType.Primary` for artifact property types, or `KafeType.Secondary` for shards, requirements, etc.
  (because `Primary` is `shard`, `req`, etc.).
- Must be in dash-case.
- Optional. If not provided explicitly, is generated based on the .NET type's name.
- Just like `Title` it isn't based on convention. The type must implement the appropriate interface
  (e.g., `IRequirement`).
- Typically looks like this:

  ```csharp
  public static string Moniker { get; } = "dash-case-type";

  public static IRequirement.Moniker {get;} = "dash-case-type";
  ```

---

For example, an entity might need to have **all** of these names:

```csharp
public class ProjectInfo : IEntity
{
    public Hrib Id { get; init; }

    public LocalizedString Name { get; init; }

    static LocalizedString IEntity.Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "project"),
        (Const.CzechCulture, "projekt"),
    );

    static string IEntity.Moniker { get; } = "project";
}
```
