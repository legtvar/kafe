using System;

namespace Kafe;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ModAttribute : Attribute
{
    public ModAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}
