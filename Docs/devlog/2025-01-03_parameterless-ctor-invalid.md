# Parameterless Constructors and the Invalid Field

Do you know what is the difference between this record...

```csharp
record MyAmazingRecord(
    string Id,
    SomeInnerData Data
) {
    public static readonly MyAmazingRecord Invalid = new("Invalid", new SomeInnerData(null));
    
    public MyAmazingRecord() : this(Invalid)
    {}
}

record SomeInnerData(string? Data);
```

...and this record...?

```csharp
record MyAmazingRecord(
    string Id,
    SomeInnerData Data
) {
    public static readonly MyAmazingRecord Invalid = new();
    
    public MyAmazingRecord() : this("Invalid", new SomeInnerData(null))
    {}
}

record SomeInnerData(string? Data);
```

Turns out... quite a big one.

In the first case, every instance of `MyAmazingRecord` has the same instance of `SomeInnerData` in its `Data` property,
because the auto-generated cloning constructor of records does a shallow clone.
On the other hand, in the second case, each instance of `MyAmazingRecord` has its own `SomeInnerData` instance.

You might think _"It shouldn't matter because `SomeInnerData` is immutable."_ or even _"The first version is better. It reduces meaningless instancing of `SomeInnerData`."_ And you might be right...

**...unless you JSON-deserialize instances of `MyAmazingRecord`!**

If you use Newtonsoft.Json and rely on default behavior, the `SomeInnerData` serialization contract will have
`ObjectCreationHandling` set to `Auto`.
That means that, during deserialization, Newtonsoft.Json won't create a new instance of `SomeInnerData` for
`MyAmazingRecord.Data` if one already got assigned to it in the parameterless constructor.
Therefore, in the first version of `MyAmazingRecord`, it uses the instance from the `Invalid` field.
Newtonsoft.Json doesn't care that `MyAmazingRecord` is a `record` type because `record`s are just C# syntax sugar.
Underneath, `SomeInnerData.Data` still has a setter and `Newtonsoft.Json` will use it to populate the instance with
deserialized data.
So... it changes it to whatever happens to be deserialized right now.

What this all means is that, in the first version, **ALL deserialized instances of `MyAmazingRecord` have the same
`SomeInnerData`**.

How bad can that be? Oh boy... I found out about this in `EntityPermissionEventProjection` because users had permissions
they were not supposed to have.
And if that isn't bad enough, consider that I used the same 
_parameter-less ctor cloning `Invalid`_ pattern for all of KAFE's `record`s.
I hope it didn't cause too much havoc because I have not way of finding out
what this caused.

Glad that's fixed though.
